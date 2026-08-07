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
    private static readonly Color CurbColor = Color.Black;

    private const float SkyHeightFraction = 0.4f;
    private const float FarRoadHalfWidthFraction = 0.1f;
    private const float NearRoadHalfWidthFraction = 0.45f;
    private const float FarCurbWidthPixels = 2f;
    private const float NearCurbWidthPixels = 16f;

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
        int horizonRow = (int)(viewport.Height * SkyHeightFraction);
        int groundRowCount = viewport.Height - horizonRow;
        int roadCenterX = viewport.Width / 2;
        float farRoadHalfWidthPixels = viewport.Width * FarRoadHalfWidthFraction;
        float nearRoadHalfWidthPixels = viewport.Width * NearRoadHalfWidthFraction;

        GraphicsDevice.Clear(SkyColor);

        _spriteBatch.Begin();

        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, viewport.Width, horizonRow), SkyColor);
        _spriteBatch.Draw(_pixel, new Rectangle(0, horizonRow, viewport.Width, groundRowCount), GrassColor);

        if (groundRowCount > 0)
        {
            for (int screenRow = horizonRow; screenRow < viewport.Height; screenRow++)
            {
                float depthFromHorizon = (screenRow - horizonRow) / (float)groundRowCount;
                int halfRoadWidth = (int)(farRoadHalfWidthPixels
                    + depthFromHorizon * (nearRoadHalfWidthPixels - farRoadHalfWidthPixels));
                int curbWidth = (int)(FarCurbWidthPixels
                    + depthFromHorizon * (NearCurbWidthPixels - FarCurbWidthPixels));

                _spriteBatch.Draw(_pixel, new Rectangle(roadCenterX - halfRoadWidth - curbWidth, screenRow, curbWidth, 1), CurbColor);
                _spriteBatch.Draw(_pixel, new Rectangle(roadCenterX - halfRoadWidth, screenRow, halfRoadWidth * 2, 1), RoadColor);
                _spriteBatch.Draw(_pixel, new Rectangle(roadCenterX + halfRoadWidth, screenRow, curbWidth, 1), CurbColor);
            }
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
