﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using FreeTypeSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static FreeTypeSharp.FT;

namespace MonoRivUI;

/// <summary>
/// Represents a scalable font.
/// </summary>
public class ScalableFont : IDisposable
{
    /// <summary>
    /// The safe factor for the text.
    /// </summary>
    /// <remarks>
    /// The safe factor is the factor by which the base character dimensions
    /// is scaled to ensure that it fits the bounds.
    /// </remarks>
    public const float SafeFactor = 1.6f;

    private const char BaseChar = 'A';
    private const int TextureDims = 1024;
    private const int DpiX = 150;
    private const int DpiY = 150;

    private static readonly FreeTypeLibrary Library = new();

    private string path;

    private FreeTypeFaceFacade face = default!;
    private List<Texture2D> textures = new();
    private Dictionary<char, GlyphData> glyphDatas = new();

    private int size;
    private int minSize = 1;
    private int maxSize = int.MaxValue;

    private bool autoResize;
    private uint height;
    private bool disposed;

    private int firstSupportedCodePoint;
    private int lastSupportedCodePoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScalableFont"/> class.
    /// </summary>
    /// <param name="path">The relative path to the font.</param>
    /// <param name="size">The size of the font.</param>
    /// <param name="firstSupportedCodePoint">
    /// The first supported character in the font.
    /// </param>
    /// <param name="lastSupportedCodePoint">
    /// The last supported character in the font.
    /// </param>
    public unsafe ScalableFont(
        string path,
        int size,
        int firstSupportedCodePoint = 0x20,
        int lastSupportedCodePoint = 0x1FF)
    {
        this.path = GetAbsoluteFontPath(path);
        this.size = size;

        this.firstSupportedCodePoint = firstSupportedCodePoint;
        this.lastSupportedCodePoint = lastSupportedCodePoint;

        ScreenController.ScreenChanged += (s, e) =>
        {
            if (this.AutoResize)
            {
                this.Load();
            }
        };

        this.Load();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScalableFont"/> class.
    /// </summary>
    /// <param name="path">The relative path to the font.</param>
    /// <param name="size">The size of the font.</param>
    /// <param name="supportedCodePoints">
    /// The supported code points in the font.
    /// </param>
    protected ScalableFont(
        string path,
        int size,
        (int First, int Last) supportedCodePoints)
        : this(path, size, supportedCodePoints.First, supportedCodePoints.Last)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="ScalableFont"/> class.
    /// </summary>
    ~ScalableFont()
    {
        this.Dispose(false);
    }

    /// <summary>
    /// Occurs when the font is reloaded.
    /// </summary>
    public event EventHandler? Reloaded;

    /// <summary>
    /// Gets or sets the size of the font.
    /// </summary>
    public int Size
    {
        get => this.size;
        set
        {
            if (this.size != value)
            {
                return;
            }

            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "The size of the font must be greater than 0.");
            }

            this.size = value;
            this.Load();
        }
    }

    /// <summary>
    /// Gets or sets the minimum size of the font.
    /// </summary>
    public int MinSize
    {
        get => this.minSize;
        set
        {
            if (this.minSize != value)
            {
                return;
            }

            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "The minimum size of the font must be greater than 0.");
            }

            if (value > this.maxSize)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "The minimum size of the font must not be greater than the maximum size.");
            }

            this.minSize = value;
            this.Load();
        }
    }

    /// <summary>
    /// Gets or sets the maximum size of the font.
    /// </summary>
    public int MaxSize
    {
        get => this.maxSize;
        set
        {
            if (this.maxSize != value)
            {
                return;
            }

            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "The maximum size of the font must be greater than 0.");
            }

            if (value < this.minSize)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "The maximum size of the font must not be less than the minimum size.");
            }

            this.maxSize = value;
            this.Load();
        }
    }

    /// <summary>
    /// Gets or sets the spacing between characters.
    /// </summary>
    public float Spacing { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the text
    /// should be auto-resized when the screen size changes.
    /// </summary>
    public bool AutoResize
    {
        get => this.autoResize;
        set
        {
            if (this.autoResize == value)
            {
                return;
            }

            this.autoResize = value;
            this.Load();
        }
    }

    /// <summary>
    /// Gets the dimensions of the base character.
    /// </summary>
    public Vector2 BaseCharDimensions => this.glyphDatas[BaseChar].TextureCoords.Size.ToVector2();

    /// <summary>
    /// Gets the safe dimensions of the text.
    /// </summary>
    /// <remarks>
    /// The safe dimensions are the dimensions of the text that are guaranteed to be fitted.
    /// </remarks>
    // TODO: This is a temporary solution.
    public Vector2 SafeDimensions => this.BaseCharDimensions * SafeFactor;

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Reloads the font, optionally changing its path.
    /// </summary>
    /// <param name="newPath">
    /// The new path to the font. If <see langword="null"/>, the current path is used.
    /// </param>
    /// <param name="firstSupportedCodePoint">
    /// The first supported character in the font. If <see langword="null"/>,
    /// the current first supported character is used.
    /// </param>
    /// <param name="lastSupportedCodePoint">
    /// The last supported character in the font. If <see langword="null"/>,
    /// the current last supported character is used.
    /// </param>
    public void Reload(
        string? newPath = null,
        int? firstSupportedCodePoint = null,
        int? lastSupportedCodePoint = null)
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);

        this.firstSupportedCodePoint = firstSupportedCodePoint ?? this.firstSupportedCodePoint;
        this.lastSupportedCodePoint = lastSupportedCodePoint ?? this.lastSupportedCodePoint;

        if (newPath is not null)
        {
            this.path = GetAbsoluteFontPath(newPath);
        }

        this.Load();
        this.Reloaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Draws the text.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="position">The position to draw the text at.</param>
    /// <param name="color">The color of the text.</param>
    /// <param name="spacing">The spacing between characters.</param>
    public void DrawString(string text, Vector2 position, Color color, float? spacing = null)
    {
        this.DrawString(text, position, color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f, spacing);
    }

    /// <summary>
    /// Draws the text.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="position">The position to draw the text at.</param>
    /// <param name="color">The color of the text.</param>
    /// <param name="rotation">The rotation of the text.</param>
    /// <param name="origin">The origin of the text.</param>
    /// <param name="scale">The scale of the text.</param>
    /// <param name="effects">The effects of the text.</param>
    /// <param name="layerDepth">The layer depth of the text.</param>
    /// <param name="spacing">The spacing between characters.</param>
    public void DrawString(
        string text,
        Vector2 position,
        Color color,
        float rotation,
        Vector2 origin,
        float scale,
        SpriteEffects effects,
        float layerDepth,
        float? spacing = null)
    {
        var sb = SpriteBatchController.SpriteBatch;
        spacing ??= this.AutoResize ? GetScaled(this.Spacing) : this.Spacing;

        Vector2 currentPosition = position;
        Vector2 currentOffset = Vector2.Zero;
        Vector2 advance = rotation == 0.0f ? Vector2.UnitX : new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));

        for (int i = 0; i < text.Length; i++)
        {
            char currentChar = text[i];

            if (this.glyphDatas.TryGetValue(currentChar, out var glyphData) && glyphData.TextureIndex is int index)
            {
                Texture2D texture = this.textures[index];
                Vector2 drawOffset;
                drawOffset.X = ((glyphData.Offset.X * advance.X) - (glyphData.Offset.Y * advance.Y)) * scale;
                drawOffset.Y = ((glyphData.Offset.X * advance.Y) + (glyphData.Offset.Y * advance.X)) * scale;
                sb.Draw(texture, currentPosition + currentOffset + drawOffset, glyphData.TextureCoords, color, rotation, origin, scale, effects, layerDepth);
            }

            currentPosition += glyphData.HorizontalAdvance * advance * scale;
            currentPosition += new Vector2(spacing.Value * scale, 0);
        }
    }

    /// <summary>
    /// Measures the size of the text.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="spacing">
    /// The spacing between characters. If not specified,
    /// the <see cref="Spacing"/> property is used.
    /// </param>
    /// <returns>The size of the text.</returns>
    public Vector2 MeasureString(string text, float? spacing = null)
    {
        return this.MeasureString(text, out _, spacing);
    }

    /// <summary>
    /// Measures the size of the text.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="heightOffset">
    /// The difference between the height of the text and the height of the base character.
    /// </param>
    /// <param name="spacing">
    /// The spacing between characters. If not specified,
    /// the <see cref="Spacing"/> property is used.
    /// </param>
    /// <returns>The size of the text.</returns>
    public Vector2 MeasureString(string text, out float heightOffset, float? spacing = null)
    {
        if (string.IsNullOrEmpty(text))
        {
            heightOffset = 0.0f;
            return Vector2.Zero;
        }

        spacing ??= this.AutoResize ? GetScaled(this.Spacing) : this.Spacing;

        var result = Vector2.Zero;
        result -= new Vector2(spacing.Value, 0);

        foreach (char c in text)
        {
            if (!this.glyphDatas.TryGetValue(c, out var data))
            {
                continue;
            }

            result.X += data.HorizontalAdvance + spacing.Value;
            result.Y = Math.Max(result.Y, this.glyphDatas[c].TextureCoords.Height);
        }

        heightOffset = result.Y - this.glyphDatas[BaseChar].TextureCoords.Height;

        return result.ClampVec(Vector2.Zero, new Vector2(float.MaxValue));
    }

    /// <summary>
    /// Measures the size of the text.
    /// </summary>
    /// <param name="sb">The string builder to measure.</param>
    /// <returns>The size of the text.</returns>
    public Vector2 MeasureString(StringBuilder sb)
    {
        return this.MeasureString(sb.ToString(), out var _);
    }

    /// <summary>
    /// Measures the size of the text.
    /// </summary>
    /// <param name="sb">The string builder to measure.</param>
    /// <param name="heightOffset">
    /// The difference between the height of the text and the height of the base character.
    /// </param>
    /// <returns>The size of the text.</returns>
    public Vector2 MeasureString(StringBuilder sb, out float heightOffset)
    {
        return this.MeasureString(sb.ToString(), out heightOffset);
    }

    /// <summary>
    /// Disposes the object.
    /// </summary>
    /// <param name="disposing">A value indicating whether the object is disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                foreach (var texture in this.textures)
                {
                    texture.Dispose();
                }

                this.textures.Clear();
            }

            this.disposed = true;
        }
    }

    private static int GetScaled(float size)
    {
        var screenScale = ScreenController.Scale;
        return (int)Math.Ceiling(size * Math.Min(screenScale.X, screenScale.Y));
    }

    private static string GetAbsoluteFontPath(string relativePath)
    {
        string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        relativePath = relativePath.Replace('\\', Path.DirectorySeparatorChar);

        return Path.GetFullPath(Path.Combine(assemblyLocation, relativePath));
    }

    private unsafe void Load()
    {
        if (this.firstSupportedCodePoint > this.lastSupportedCodePoint)
        {
            throw new ArgumentException(
                "The first supported character must not be greater than the last supported character.");
        }

        var size = this.AutoResize ? GetScaled(this.size) : this.size;
        size = Math.Clamp(size, this.minSize, this.maxSize);

        FT_FaceRec_* facePtr;
        var error = FT_New_Face(Library.Native, (byte*)Marshal.StringToHGlobalAnsi(this.path), IntPtr.Zero, &facePtr);
        FTError.ThrowIfError(this, error);

        this.face = new FreeTypeFaceFacade(Library, facePtr);

        this.Render(size);
    }

    private unsafe void Render(int size)
    {
        this.textures.ForEach(x => x.Dispose());
        this.textures.Clear();
        this.glyphDatas.Clear();

        this.face.SelectCharSize(size, DpiX, DpiY);

        uint nextY = 0;
        GlyphData data;
        Texture2D texture;
        Vector2 currentCoords = Vector2.Zero;
        var pixelBuf = new uint[TextureDims * TextureDims];
        var result = new List<Texture2D>();

        FT_GlyphSlotRec_* glyph = this.LoadGlyph(BaseChar);
        this.height = glyph->bitmap.rows;

        for (var c = (char)this.firstSupportedCodePoint; c <= this.lastSupportedCodePoint; c++)
        {
            glyph = this.LoadGlyph(c);

            if (glyph == null)
            {
                continue;
            }

            if (glyph->metrics.width == IntPtr.Zero || glyph->metrics.height == IntPtr.Zero)
            {
                data = new GlyphData(
                    Vector2.Zero,
                    this.face.GlyphMetricHorizontalAdvance,
                    Rectangle.Empty,
                    null);
                this.glyphDatas.Add(c, data);
                continue;
            }

            FT_Bitmap_ bitmap = glyph->bitmap;
            byte* bitmapBuf = bitmap.buffer;
            uint glyphWidth = bitmap.width;
            uint glyphHeight = bitmap.rows;

            if (currentCoords.X + glyphWidth + 2 >= TextureDims)
            {
                currentCoords.X = 0;
                currentCoords.Y += nextY;
                nextY = 0;
            }

            nextY = Math.Max(nextY, glyphHeight + 2);

            if (currentCoords.Y + glyphHeight + 2 >= TextureDims)
            {
                currentCoords.X = currentCoords.Y = 0;
                texture = new Texture2D(ScreenController.GraphicsDevice, TextureDims, TextureDims);
                texture.SetData(pixelBuf);
                this.textures.Add(texture);
                pixelBuf = new uint[TextureDims * TextureDims];
            }

            data = new GlyphData(
                new Vector2(this.face.GlyphBitmapLeft, this.height - this.face.GlyphBitmapTop),
                this.face.GlyphMetricHorizontalAdvance,
                new Rectangle((int)currentCoords.X, (int)currentCoords.Y, (int)glyphWidth, (int)glyphHeight),
                this.textures.Count);

            this.glyphDatas.Add(c, data);

            byte* alphaPtr = bitmapBuf;
            for (int y = 0; y < glyphHeight; y++)
            {
                for (int x = 0; x < glyphWidth; x++)
                {
                    byte alpha = *alphaPtr++;
                    pixelBuf[(int)currentCoords.X + x + (((int)currentCoords.Y + y) * TextureDims)] = (uint)(alpha << 24) | 0x00ffffff;
                }
            }

            currentCoords.X += glyphWidth + 2;
        }

        texture = new Texture2D(ScreenController.GraphicsDevice, TextureDims, TextureDims);
        texture.SetData(pixelBuf);
        this.textures.Add(texture);
    }

    private unsafe FT_GlyphSlotRec_* LoadGlyph(char c)
    {
        uint glyphIndex = this.face.GetCharIndex(c);

        if (glyphIndex == 0)
        {
            return null;
        }

        var error = FT_Load_Glyph(this.face.FaceRec, glyphIndex, FT_LOAD.FT_LOAD_DEFAULT);
        FTError.ThrowIfError(this, error);

        FT_GlyphSlotRec_* glyph = this.face.FaceRec->glyph;
        error = FT_Render_Glyph(glyph, FT_Render_Mode_.FT_RENDER_MODE_NORMAL);
        FTError.ThrowIfError(this, error);

        return glyph;
    }

    private readonly record struct GlyphData(
        Vector2 Offset,
        float HorizontalAdvance,
        Rectangle TextureCoords,
        int? TextureIndex);

    /// <summary>
    /// Represents an error that occurred in FreeType.
    /// </summary>
    public class FTError : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FTError"/> class.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="relativePath">The relative path to the font.</param>
        public FTError(FreeTypeException exception, string relativePath)
        {
            this.Exception = exception;
            this.RelativePath = relativePath;
        }

        /// <summary>
        /// Gets the exception that occurred.
        /// </summary>
        public FreeTypeException Exception { get; }

        /// <summary>
        /// Gets the relative path to the font.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Gets the absolute path to the font.
        /// </summary>
        public string AbsolutePath => Path.GetFullPath(this.RelativePath);

        /// <inheritdoc/>
        public override string Message => "An error occurred in FreeType " +
            $"while loading the font \"{this.AbsolutePath}\".\n" +
            $"Error: {this.Exception.Message}";

        /// <summary>
        /// Throws an exception if an error occurred.
        /// </summary>
        /// <param name="font">The font that the error occurred in.</param>
        /// <param name="error">The error that occurred.</param>
        /// <exception cref="FTError">
        /// The error that occurred enclosed in custom exception.
        /// </exception>
        internal static void ThrowIfError(ScalableFont font, FT_Error error)
        {
            if (error != FT_Error.FT_Err_Ok)
            {
                throw new FTError(new FreeTypeException(error), font.path);
            }
        }
    }
}
