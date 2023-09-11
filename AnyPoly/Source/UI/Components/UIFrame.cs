using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AnyPoly.UI;

/// <summary>
/// Represents a UI frame component.
/// </summary>
internal class UIFrame : UIComponent
{
    private const int NumberOfLines = 4;

    private readonly LineData[] lines = new LineData[NumberOfLines];
    private readonly UIContainer innerContainer;

    private float relativeThickness = 0.01f;
    private Color color;

    private bool isUpdateNeeded = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIFrame"/> class.
    /// </summary>
    /// <param name="color">The color of the frame lines.</param>
    public UIFrame(Color color)
    {
        this.color = color;

        this.innerContainer = new UIContainer()
        {
            Parent = this,
            Transform = { Type = TransformType.Absolute },
        };

        this.Transform.Recalculated += this.Transform_Recalculated;

        this.isUpdateNeeded = true;
    }

    /// <summary>
    /// Gets or sets the relative thickness of the frame lines.
    /// </summary>
    /// <remarks>
    /// The thickness is relative to the shorter size of the frame.
    /// </remarks>
    public float RelativeThickness
    {
        get => this.relativeThickness;
        set
        {
            if (this.relativeThickness == value)
            {
                return;
            }

            this.relativeThickness = value;
            this.isUpdateNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the color of the frame lines.
    /// </summary>
    public Color Color
    {
        get => this.color;
        set
        {
            if (this.color == value)
            {
                return;
            }

            this.color = value;
        }
    }

    /// <summary>
    /// Gets the read-only inner container of the frame.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The inner container serves as a designated rectangle within
    /// the frame where child UI elements can be positioned and organized.
    /// </para>
    /// <para>
    /// It is automatically created when <see cref="UIFrame"/>
    /// is initialized and is intended to provide a structured
    /// layout for nested components.
    /// </para>
    /// <para>
    /// Since it is automatically generated and managed by the
    /// <see cref="UIFrame"/> component, it is provided
    /// as a read-only property to avoid external modification.
    /// </para>
    /// </remarks>
    public IUIReadOnlyComponent InnerContainer => this.innerContainer;

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (this.isUpdateNeeded)
        {
            this.UpdateLines();
            this.UpdateInnerRectangle();
            this.isUpdateNeeded = false;
        }

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        foreach (LineData line in this.lines)
        {
            SpriteBatchController.SpriteBatch.Draw(
                texture: SpriteBatchController.WhitePixel,
                position: line.Start,
                sourceRectangle: null,
                color: this.color,
                rotation: line.Angle,
                origin: Vector2.Zero,
                scale: line.Scale,
                effects: SpriteEffects.None,
                layerDepth: 0.0f);
        }

        base.Draw(gameTime);
    }

    private void UpdateLines()
    {
        Rectangle rect = this.Transform.ScaledRectangle;
        int scaledThickness = this.GetScaledThickness();

        // Define pairs of start and end points for lines
        var lineEndpoints = new Vector2[]
        {
            new Vector2(rect.Left, rect.Top),
            new Vector2(rect.Right, rect.Top),
            new Vector2(rect.Right, rect.Bottom),
            new Vector2(rect.Left, rect.Bottom),
        };

        for (int i = 0; i < NumberOfLines; i++)
        {
            Vector2 start = lineEndpoints[i];
            Vector2 end = lineEndpoints[(i + 1) % NumberOfLines];

            float length = Vector2.Distance(start, end);
            float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

            this.lines[i] = new LineData()
            {
                Start = start,
                Angle = angle,
                Scale = new Vector2(length, scaledThickness),
            };
        }
    }

    private void UpdateInnerRectangle()
    {
        Rectangle rect = this.Transform.ScaledRectangle;
        int scaledThickness = this.GetScaledThickness();

        var location = rect.Location += new Point(scaledThickness);
        var size = rect.Size -= new Point(2 * scaledThickness);

        this.innerContainer.Transform.ScaledLocation = location;
        this.innerContainer.Transform.ScaledSize = size;
    }

    private int GetScaledThickness()
    {
        Point size = this.Transform.ScaledSize
            .ToVector2()
            .Scale(this.relativeThickness)
            .ToPoint()
            .Clamp(this.Transform.MinSize, this.Transform.MaxSize);

        return Math.Min(size.X, size.Y);
    }

    private void Transform_Recalculated(object? sender, EventArgs e)
    {
        this.UpdateLines();
        this.UpdateInnerRectangle();
        this.isUpdateNeeded = false;
    }

    private struct LineData
    {
        public Vector2 Start;
        public float Angle;
        public Vector2 Scale;
    }
}
