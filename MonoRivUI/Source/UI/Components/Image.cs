﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoRivUI;

/// <summary>
/// Represents an image component.
/// </summary>
public class Image : TextureComponent, IButtonContent<Image>
{
    // We use a reference counter to keep track of how many times a texture is used.
    // This way, we can dispose of the texture when it is no longer used anywhere.
    private static readonly Dictionary<string, int> TextureRefereceCounter = new();

    // We cache the pixels of the textures
    // so that we don't have to load them every time we need them.
    private static readonly Dictionary<string, Lazy<Color[]>> CachedTexturePixels = new();

    private readonly string path;
    private readonly Lazy<Color[]> texturePixels;

    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class.
    /// </summary>
    /// <param name="relativePath">
    /// The relative path within the <c>"Content/Images/"</c>
    /// directory to the image, without the file extension.
    /// </param>
    /// <remarks>
    /// This constructor also sets the <see cref="Transform.Ratio"/>
    /// to the ratio of the image.
    /// </remarks>
    public Image(string relativePath)
    {
        this.path = $"Images/{relativePath}";
        this.texturePixels = new Lazy<Color[]>(this.LoadImagePixels);
        this.AddTextureToReferenceCounter();
    }

    /// <summary>
    /// Gets the pixels of the texture.
    /// </summary>
    public Color[] TexturePixels => this.texturePixels.Value;

    /// <summary>
    /// Checks if the cursor is over a non-transparent pixel in the image.
    /// </summary>
    /// <param name="mousePosition">The position of the cursor.</param>
    /// <returns>
    /// <see langword="true"/> if the cursor is over a non-transparent pixel;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IButtonContent<Image>.IsButtonContentHovered(Point mousePosition)
    {
        Point location = this.Transform.Location;
        Point size = this.Transform.Size;
        Point mouseOffset = mousePosition - location;
        int mouseOffsetX = mouseOffset.X * this.Texture.Width / size.X;
        int mouseOffsetY = mouseOffset.Y * this.Texture.Height / size.Y;
        int index = (mouseOffsetY * this.Texture.Width) + mouseOffsetX;
        Color[] texturePixels = this.TexturePixels;
        return index >= 0 && index < texturePixels.Length
            && texturePixels[index].A > 0;
    }

    /// <summary>
    /// Disposes the texture if it is no longer used anywhere.
    /// </summary>
    public void Dispose()
    {
        if (--TextureRefereceCounter[this.path] == 0)
        {
            _ = TextureRefereceCounter.Remove(this.path);
            _ = CachedTexturePixels.Remove(this.path);
            this.Texture.Dispose();
        }
    }

    /// <inheritdoc/>
    public override void Load()
    {
        this.Texture = ContentController.Content.Load<Texture2D>(this.path);
        this.Transform.Ratio = this.TextureRatio;
        this.IsLoaded = true;
    }

    private void AddTextureToReferenceCounter()
    {
        if (TextureRefereceCounter.ContainsKey(this.path))
        {
            TextureRefereceCounter[this.path]++;
        }
        else
        {
            TextureRefereceCounter.Add(this.path, 1);
        }
    }

    private Color[] LoadImagePixels()
    {
        if (CachedTexturePixels.ContainsKey(this.path!))
        {
            return CachedTexturePixels[this.path!].Value;
        }

        var pixels = new Color[this.Texture.Width * this.Texture.Height];
        this.Texture.GetData(pixels);
        CachedTexturePixels.Add(this.path, new Lazy<Color[]>(() => pixels));
        return pixels;
    }
}
