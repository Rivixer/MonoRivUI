using System;
using System.Drawing.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoRivUI;

/// <summary>
/// Represents a text input component.
/// </summary>
public class TextInput : Component
{
    private const int CursorBlinkTime = 500; // ms

    private readonly Text text;
    private readonly SolidColor caret;

    private int caretPosition;
    private float? caretEnableTime;

    private Text? placeholder;
    private float placeholderOpacity = 1.0f;

    private uint maxLength = int.MaxValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextInput"/> class.
    /// </summary>
    /// <param name="font">The font of the text.</param>
    /// <param name="color">The color of the displayed text.</param>
    /// <param name="caretColor">The color of the caret.</param>
    public TextInput(ScalableFont font, Color color, Color caretColor)
    {
        this.text = new Text(font, color)
        {
            Parent = this,
            Value = string.Empty,
        };

        this.caret = new SolidColor(caretColor)
        {
            Parent = this.text,
            AutoDraw = false,
            Transform =
            {
                RelativeSize = new Vector2(0.9f),
                Alignment = Alignment.Left,
                Ratio = new Ratio(1, 14),
            },
        };
    }

    /// <summary>
    /// An event raised when the input text has been changed.
    /// </summary>
    public event EventHandler<TextInputEventArgs>? TextChanged;

    /// <summary>
    /// An event raised when the text input has been sent.
    /// </summary>
    public event EventHandler<TextInputEventArgs>? TextInputSent;

    /// <summary>
    /// Gets a value indicating whether the text input is selected.
    /// </summary>
    public bool IsSelected { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether
    /// the text input should be cleared after sending.
    /// </summary>
    public bool ClearAfterSend { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether
    /// the text input should be deselected after sending.
    /// </summary>
    public bool DeselectAfterSend { get; set; } = true;

    /// <summary>
    /// Gets the value of the text input.
    /// </summary>
    public string Value => this.text.Value;

    /// <summary>
    /// Gets or sets a value indicating
    /// maximum length of the text input.
    /// </summary>
    public uint MaxLength
    {
        get => this.maxLength;
        set
        {
            if (this.maxLength == value)
            {
                return;
            }

            this.maxLength = value;

            if (this.text.Value.Length > this.maxLength)
            {
                this.text.Value = this.text.Value[..(int)this.maxLength];
                this.OnTextChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the input text alignment.
    /// </summary>
    /// <remarks>
    /// It also affects the placeholder alignment.
    /// </remarks>
    public Alignment TextAlignment
    {
        get => this.text.TextAlignment;
        set
        {
            this.text.TextAlignment = value;
            if (this.placeholder is not null)
            {
                this.placeholder.TextAlignment = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the text shrink mode.
    /// </summary>
    /// <remarks>
    /// It also affects the placeholder shrink mode.
    /// <inheritdoc cref="Text.TextShrink"/>
    /// </remarks>
    public TextShrinkMode TextShrink
    {
        get => this.text.TextShrink;
        set
        {
            this.text.TextShrink = value;
            if (this.placeholder is not null)
            {
                this.placeholder.TextShrink = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the text case.
    /// </summary>
    public TextCase Case
    {
        get => this.text.Case;
        set
        {
            this.text.Case = value;
            if (this.placeholder is not null)
            {
                this.placeholder.Case = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether
    /// the caret height should be adjusted to the font height.
    /// </summary>
    /// <remarks>
    /// If it is set to <see langword="true"/>,
    /// the caret height will be adjusted to the font
    /// <see cref="ScalableFont.SafeDimensions"/>'s Y value.
    /// Otherwise, the relative size of the caret will be set to 90%.
    /// </remarks>
    public bool AdjustCaretHeightToFont { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text.
    /// </summary>
    public string? Placeholder
    {
        get => this.placeholder?.Value;
        set
        {
            if (this.placeholder?.Value == value)
            {
                return;
            }

            if (value is null)
            {
                this.placeholder = null;
            }
            else if (this.placeholder is not null)
            {
                this.placeholder.Value = value;
            }
            else
            {
                this.placeholder = new Text(this.text.Font, this.text.Color * this.placeholderOpacity)
                {
                    Parent = this,
                    Value = value,
                    TextAlignment = this.TextAlignment,
                    TextShrink = this.TextShrink,
                    Transform =
                    {
                        RelativeOffset = this.text.Transform.RelativeOffset,
                        RelativeSize = this.text.Transform.RelativeSize,
                    },
                };
            }
        }
    }

    /// <summary>
    /// Gets or sets the opacity of the placeholder text.
    /// </summary>
    public float PlaceholderOpacity
    {
        get => this.placeholderOpacity;
        set
        {
            this.placeholderOpacity = value;
            if (this.placeholder is not null)
            {
                this.placeholder.Color = this.text.Color * this.placeholderOpacity;
            }
        }
    }

    /// <summary>
    /// Gets or sets the scale of the text.
    /// </summary>
    /// <remarks>
    /// It also affects the placeholder scale.
    /// </remarks>
    public float Scale
    {
        get => this.text.Scale;
        set
        {
            this.text.Scale = value;
            if (this.placeholder is not null)
            {
                this.placeholder.Scale = value;
            }
        }
    }

    /// <summary>
    /// Selects the text input.
    /// </summary>
    public void Select()
    {
        if (!this.IsSelected)
        {
            ScreenController.GameWindow.TextInput += this.Window_TextInput;
            ScreenController.GameWindow.KeyDown += this.Window_KeyDown;
            this.IsSelected = true;
            this.caretEnableTime = 0.0f;
        }
    }

    /// <summary>
    /// Deselected the text input.
    /// </summary>
    public void Deselect()
    {
        if (this.IsSelected)
        {
            ScreenController.GameWindow.TextInput -= this.Window_TextInput;
            ScreenController.GameWindow.KeyDown -= this.Window_KeyDown;
            this.IsSelected = false;
        }
    }

    /// <summary>
    /// Sets the text of the text input.
    /// </summary>
    /// <param name="text">The text to be set.</param>
    public void SetText(string text)
    {
        this.text.Value = text;
        this.caretPosition = text.Length;
        this.OnTextChanged();
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        if (MouseController.IsLeftButtonClicked())
        {
            this.HandleSelection();
        }

        if (this.IsSelected)
        {
            if (KeyboardController.IsKeyHit(Keys.Enter))
            {
                this.HandleSendingText();
            }
            else
            {
                var elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
                this.caretEnableTime += elapsedSeconds;
                this.AdjustCaretHeight();
                this.UpdateCaretPosition();
            }
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

        if (this.text.Value.Length == 0)
        {
            this.placeholder?.Draw(gameTime);
        }
        else
        {
            this.text.Draw(gameTime);
        }

        if (this.IsSelected && (this.caretEnableTime * 1000.0f) % 1000 < CursorBlinkTime)
        {
            this.caret.Draw(gameTime);
        }
    }

    private void HandleSelection()
    {
        if (MouseController.IsComponentFocused(this))
        {
            this.Select();
        }
        else
        {
            this.Deselect();
        }
    }

    private void HandleSendingText()
    {
        var args = new TextInputEventArgs(this.text.Value);
        this.TextInputSent?.Invoke(this, args);

        if (this.ClearAfterSend)
        {
            this.text.Value = string.Empty;
            this.caretPosition = 0;
            this.OnTextChanged();
        }

        if (this.DeselectAfterSend)
        {
            this.Deselect();
        }
    }

    private void UpdateCaretPosition()
    {
        float offset = this.text.MeasureDimensions(0, this.caretPosition).X;
        float totalTextWidth = this.text.Dimensions.X;

        if (this.TextAlignment.HasFlag(Alignment.Right))
        {
            offset += this.Transform.Size.X - totalTextWidth;
        }
        else if (!this.TextAlignment.HasFlag(Alignment.Left))
        {
            offset += (this.Transform.Size.X - totalTextWidth) / 2f;
        }

        this.caret.Transform.SetRelativeOffsetFromAbsolute(x: offset);
    }

    private void AdjustCaretHeight()
    {
        if (this.AdjustCaretHeightToFont)
        {
            this.caret.Transform.SetRelativeSizeFromAbsolute(y: this.text.Font.SafeDimensions.Y);
        }
        else
        {
            this.caret.Transform.RelativeSize = new Vector2(0.9f);
        }
    }

    private void OnTextChanged()
    {
        var args = new TextInputEventArgs(this.text.Value);
        this.TextChanged?.Invoke(this, args);
    }

    private void Window_KeyDown(object? sender, InputKeyEventArgs e)
    {
        bool isCtrlDown = KeyboardController.IsKeyDown(Keys.LeftControl)
            || KeyboardController.IsKeyDown(Keys.RightControl);

        switch (e.Key)
        {
            case Keys.Left:
                this.caretPosition = Math.Max(this.caretPosition - 1, 0);
                this.caretEnableTime = 0.0f;
                break;
            case Keys.Right:
                this.caretPosition = Math.Min(this.caretPosition + 1, this.text.Value.Length);
                this.caretEnableTime = 0.0f;
                break;
            case Keys.Back:
                if (this.caretPosition > 0)
                {
                    if (isCtrlDown)
                    {
                        int previousNonSpace = this.caretPosition - 1;
                        while (previousNonSpace >= 0 && this.text.Value[previousNonSpace] == ' ')
                        {
                            previousNonSpace--;
                        }

                        while (previousNonSpace >= 0 && this.text.Value[previousNonSpace] != ' ')
                        {
                            previousNonSpace--;
                        }

                        int removeStart = previousNonSpace + 1;
                        int removeCount = this.caretPosition - removeStart;
                        if (removeCount > 0)
                        {
                            this.text.Value = this.text.Value.Remove(removeStart, removeCount);
                            this.caretPosition = removeStart;
                        }
                    }
                    else
                    {
                        this.text.Value = this.text.Value.Remove(this.caretPosition - 1, 1);
                        this.caretPosition--;
                    }
                }

                break;
            case Keys.Delete:
                if (isCtrlDown)
                {
                    int nextNonSpace = this.caretPosition;
                    while (nextNonSpace < this.text.Value.Length && this.text.Value[nextNonSpace] == ' ')
                    {
                        nextNonSpace++;
                    }

                    while (nextNonSpace < this.text.Value.Length && this.text.Value[nextNonSpace] != ' ')
                    {
                        nextNonSpace++;
                    }

                    int removeStart = this.caretPosition;
                    int removeCount = nextNonSpace - removeStart;
                    if (removeCount > 0)
                    {
                        this.text.Value = this.text.Value.Remove(removeStart, removeCount);
                    }
                }
                else
                {
                    if (this.caretPosition < this.text.Value.Length)
                    {
                        this.text.Value = this.text.Value.Remove(this.caretPosition, 1);
                    }
                }

                break;
            case Keys.Home:
                this.caretPosition = 0;
                this.caretEnableTime = 0.0f;
                break;
            case Keys.End:
                this.caretPosition = this.text.Value.Length;
                this.caretEnableTime = 0.0f;
                break;
        }

        this.OnTextChanged();
    }

    private void Window_TextInput(object? sender, Microsoft.Xna.Framework.TextInputEventArgs e)
    {
        if (this.text.Value.Length < this.MaxLength && this.text.Font.MeasureString(e.Character.ToString()).X > 0)
        {
            this.text.Value = this.text.Value.Insert(this.caretPosition++, e.Character.ToString());
            this.OnTextChanged();
            this.caretEnableTime = 0.0f;
        }
    }
}
