using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PolePosition;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _pixel;

    private static readonly Color SkyColor = new(100, 160, 255);
    private static readonly Color GrassColor = new(40, 160, 60);
    private static readonly Color RoadColor = new(120, 120, 120);
    private static readonly Color ShoulderColor = Color.Black;

    private const float HorizonRatio = 0.4f;
    private const float MinRoadHalfWidthRatio = 0.1f;
    private const float MaxRoadHalfWidthRatio = 0.45f;
    private const float MinShoulderWidth = 2f;
    private const float MaxShoulderWidth = 16f;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var viewport = GraphicsDevice.Viewport;
        int horizonY = (int)(viewport.Height * HorizonRatio);
        int groundHeight = viewport.Height - horizonY;
        int cx = viewport.Width / 2;
        float minRoadHalfWidth = viewport.Width * MinRoadHalfWidthRatio;
        float maxRoadHalfWidth = viewport.Width * MaxRoadHalfWidthRatio;

        GraphicsDevice.Clear(SkyColor);

        _spriteBatch.Begin();

        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, viewport.Width, horizonY), SkyColor);
        _spriteBatch.Draw(_pixel, new Rectangle(0, horizonY, viewport.Width, groundHeight), GrassColor);

        if (groundHeight > 0)
        {
            for (int y = horizonY; y < viewport.Height; y++)
            {
                float t = (y - horizonY) / (float)groundHeight;
                int roadHalfWidth = (int)(minRoadHalfWidth + t * (maxRoadHalfWidth - minRoadHalfWidth));
                int shoulderWidth = (int)(MinShoulderWidth + t * (MaxShoulderWidth - MinShoulderWidth));

                _spriteBatch.Draw(_pixel, new Rectangle(cx - roadHalfWidth - shoulderWidth, y, shoulderWidth, 1), ShoulderColor);
                _spriteBatch.Draw(_pixel, new Rectangle(cx - roadHalfWidth, y, roadHalfWidth * 2, 1), RoadColor);
                _spriteBatch.Draw(_pixel, new Rectangle(cx + roadHalfWidth, y, shoulderWidth, 1), ShoulderColor);
            }
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
