using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AnyPoly.UI;

/// <summary>
/// Represents a UI frame component.
/// </summary>
internal class UIFrame : UIComponent
{
    private const int NumberOfLines = 4;

    private readonly LineData[] lines = new LineData[NumberOfLines];
    private readonly UIContainer innerContainer;

    private int thickness;
    private Color color;

    private bool isUpdateNeeded = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIFrame"/> class.
    /// </summary>
    /// <param name="thickness">The thickness of the frame lines.</param>
    /// <param name="color">The color of the frame lines.</param>
    public UIFrame(int thickness, Color color)
    {
        this.thickness = thickness;
        this.color = color;

        this.innerContainer = new UIContainer()
        {
            Parent = this,
            TransformType = TransformType.Absolute,
        };

        this.Transform.Recalculated += this.Transform_Recalculated;
        this.ChildAdded += this.UIFrame_ChildAdded;

        this.isUpdateNeeded = true;
    }

    /// <summary>
    /// Gets or sets the thickness of the frame lines.
    /// </summary>
    public int Thickness
    {
        get => this.thickness;
        set
        {
            if (this.thickness == value)
            {
                return;
            }

            this.thickness = value;
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
        Point scaledThickess = new Point(this.thickness).Scale(ScreenController.Scale);

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

            bool isVertical = Math.Abs(angle) is > MathF.PI / 4f and < 3 * MathF.PI / 4f;
            float destinationThickness = isVertical
                ? scaledThickess.Y
                : scaledThickess.X;

            this.lines[i] = new LineData()
            {
                Start = start,
                Angle = angle,
                Scale = new Vector2(length, destinationThickness),
            };
        }
    }

    private void UpdateInnerRectangle()
    {
        Rectangle rect = this.Transform.ScaledRectangle;
        Point scaledThickess = new Point(this.thickness).Scale(ScreenController.Scale);

        var location = new Point(rect.X + scaledThickess.X, rect.Y + scaledThickess.Y);
        var size = new Point(rect.Width - (2 * scaledThickess.X), rect.Height - (2 * scaledThickess.Y));

        this.innerContainer.Transform.ScaledLocation = location;
        this.innerContainer.Transform.ScaledSize = size;
    }

    private void Transform_Recalculated(object? sender, EventArgs e)
    {
        this.UpdateLines();
        this.UpdateInnerRectangle();
        this.isUpdateNeeded = false;
    }

    private void UIFrame_ChildAdded(object? sender, ChildChangedEventArgs e)
    {
        this.ReparentChild(e.Child, this.innerContainer);
    }

    private struct LineData
    {
        public Vector2 Start;
        public float Angle;
        public Vector2 Scale;
    }
}
