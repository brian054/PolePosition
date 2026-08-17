using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PolePosition;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _pixel;
    private float _roadScroll;

    private static readonly Color SkyColor = new(100, 160, 255);
    private static readonly Color GrassColor = new(40, 160, 60);
    private static readonly Color RoadColor = new(120, 120, 120);
    private static readonly Color CurbColor = Color.Black;
    private static readonly Color LineColor = Color.White;

    private const float SkyHeightFraction = 0.4f;
    private const float FarRoadHalfWidthFraction = 0.1f;
    private const float NearRoadHalfWidthFraction = 0.45f;
    private const float FarCurbWidthPixels = 2f;
    private const float NearCurbWidthPixels = 16f;
    private const float FarCenterLineHalfWidthPixels = 1f;
    private const float NearCenterLineHalfWidthPixels = 4f;
    private const float FarSideLineWidthPixels = 2f;
    private const float NearSideLineWidthPixels = 6f;
    private const float RoadScrollSpeed = 25f;
    private const float DashLength = 1.5f;
    private const float GapLength = 2.5f;
    private const float DepthEpsilon = 0.02f;

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
        var keyboard = Keyboard.GetState();
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Escape))
            Exit();

        if (keyboard.IsKeyDown(Keys.Enter) || keyboard.IsKeyDown(Keys.Space))
        {
            float period = DashLength + GapLength;
            _roadScroll = (_roadScroll + RoadScrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds) % period;
        }

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

                float worldZ = 1f / Math.Max(depthFromHorizon, DepthEpsilon);
                float stripePos = worldZ + _roadScroll;
                float period = DashLength + GapLength;
                float phase = stripePos % period;
                if (phase < 0f)
                    phase += period;

                if (phase < DashLength)
                {
                    int halfLineWidth = (int)(FarCenterLineHalfWidthPixels
                        + depthFromHorizon * (NearCenterLineHalfWidthPixels - FarCenterLineHalfWidthPixels));
                    halfLineWidth = Math.Max(halfLineWidth, 1);
                    int sideLineWidth = (int)(FarSideLineWidthPixels
                        + depthFromHorizon * (NearSideLineWidthPixels - FarSideLineWidthPixels));
                    sideLineWidth = Math.Max(sideLineWidth, 1);

                    _spriteBatch.Draw(
                        _pixel,
                        new Rectangle(roadCenterX - halfLineWidth, screenRow, halfLineWidth * 2, 1),
                        LineColor);
                    _spriteBatch.Draw(
                        _pixel,
                        new Rectangle(roadCenterX - halfRoadWidth, screenRow, sideLineWidth, 1),
                        LineColor);
                    _spriteBatch.Draw(
                        _pixel,
                        new Rectangle(roadCenterX + halfRoadWidth - sideLineWidth, screenRow, sideLineWidth, 1),
                        LineColor);
                }
            }
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
