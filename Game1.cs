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
    private float _currentSpeed;

    private static readonly Color SkyColor = new(100, 160, 255);
    private static readonly Color GrassColor = new(40, 160, 60);
    private static readonly Color RoadColor = new(120, 120, 120);
    private static readonly Color OuterEdgeColor = Color.Black;
    private static readonly Color CurbRed = new(220, 40, 40);
    private static readonly Color CurbWhite = Color.White;
    private static readonly Color LineColor = Color.White;
    private static readonly Color YellowLineColor = new(240, 200, 40);
    private static readonly Color CarColor = new(230, 40, 50);

    private const float SkyHeightFraction = 0.4f;
    private const float FarRoadHalfWidthFraction = 0.1f;
    private const float NearRoadHalfWidthFraction = 0.45f;
    private const float FarCurbWidthPixels = 5f;
    private const float NearCurbWidthPixels = 32f;
    private const float FarOuterEdgeWidthPixels = 1f;
    private const float NearOuterEdgeWidthPixels = 3f;
    private const float FarCenterLineHalfWidthPixels = 1f;
    private const float NearCenterLineHalfWidthPixels = 4f;
    private const float FarSideLineWidthPixels = 4f;
    private const float NearSideLineWidthPixels = 10f;
    private const float FarYellowInsetPixels = 4f;
    private const float NearYellowInsetPixels = 14f;
    private const float MaxRoadSpeed = 20f;
    private const float RoadAcceleration = 9f;
    private const float RoadDeceleration = 12f;
    private const float DashLength = 1.5f;
    private const float GapLength = 2.5f;
    private const float RumbleRedLength = 0.8f;
    private const float RumbleWhiteLength = 0.8f;
    private const float YellowDashLength = 6f;
    private const float YellowGapLength = 10f;
    private const float DepthEpsilon = 0.02f;
    private const float CarWidthFraction = 0.20f;
    private const float CarHeightFraction = 0.18f;
    private const float CarBottomMarginFraction = 0.15f;

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

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        bool accelerating = keyboard.IsKeyDown(Keys.Enter) || keyboard.IsKeyDown(Keys.Space);
        if (accelerating)
            _currentSpeed = Math.Min(MaxRoadSpeed, _currentSpeed + RoadAcceleration * dt);
        else
            _currentSpeed = Math.Max(0f, _currentSpeed - RoadDeceleration * dt);

        if (_currentSpeed > 0f)
        {
            // Wrap on the yellow period so center, rumble, and yellow stay continuous.
            float wrapPeriod = YellowDashLength + YellowGapLength;
            _roadScroll = (_roadScroll + _currentSpeed * dt) % wrapPeriod;
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
                int outerEdgeWidth = (int)(FarOuterEdgeWidthPixels
                    + depthFromHorizon * (NearOuterEdgeWidthPixels - FarOuterEdgeWidthPixels));
                outerEdgeWidth = Math.Max(outerEdgeWidth, 1);

                float worldZ = 1f / Math.Max(depthFromHorizon, DepthEpsilon);
                float stripePos = worldZ + _roadScroll;

                float period = DashLength + GapLength;
                float phase = stripePos % period;
                if (phase < 0f)
                    phase += period;
                bool centerDash = phase < DashLength;

                float rumblePeriod = RumbleRedLength + RumbleWhiteLength;
                float rumblePhase = stripePos % rumblePeriod;
                if (rumblePhase < 0f)
                    rumblePhase += rumblePeriod;
                Color rumbleColor = rumblePhase < RumbleRedLength ? CurbRed : CurbWhite;

                float yellowPeriod = YellowDashLength + YellowGapLength;
                float yellowPhase = stripePos % yellowPeriod;
                if (yellowPhase < 0f)
                    yellowPhase += yellowPeriod;
                bool yellowDash = yellowPhase < YellowDashLength;

                _spriteBatch.Draw(_pixel, new Rectangle(roadCenterX - halfRoadWidth - curbWidth - outerEdgeWidth, screenRow, outerEdgeWidth, 1), OuterEdgeColor);
                _spriteBatch.Draw(_pixel, new Rectangle(roadCenterX - halfRoadWidth - curbWidth, screenRow, curbWidth, 1), rumbleColor);
                _spriteBatch.Draw(_pixel, new Rectangle(roadCenterX - halfRoadWidth, screenRow, halfRoadWidth * 2, 1), RoadColor);
                _spriteBatch.Draw(_pixel, new Rectangle(roadCenterX + halfRoadWidth, screenRow, curbWidth, 1), rumbleColor);
                _spriteBatch.Draw(_pixel, new Rectangle(roadCenterX + halfRoadWidth + curbWidth, screenRow, outerEdgeWidth, 1), OuterEdgeColor);

                if (centerDash)
                {
                    int halfLineWidth = (int)(FarCenterLineHalfWidthPixels
                        + depthFromHorizon * (NearCenterLineHalfWidthPixels - FarCenterLineHalfWidthPixels));
                    halfLineWidth = Math.Max(halfLineWidth, 1);
                    _spriteBatch.Draw(
                        _pixel,
                        new Rectangle(roadCenterX - halfLineWidth, screenRow, halfLineWidth * 2, 1),
                        LineColor);
                }

                if (yellowDash)
                {
                    int sideLineWidth = (int)(FarSideLineWidthPixels
                        + depthFromHorizon * (NearSideLineWidthPixels - FarSideLineWidthPixels));
                    sideLineWidth = Math.Max(sideLineWidth, 1);
                    int yellowInset = (int)(FarYellowInsetPixels
                        + depthFromHorizon * (NearYellowInsetPixels - FarYellowInsetPixels));
                    yellowInset = Math.Max(yellowInset, 1);
                    _spriteBatch.Draw(
                        _pixel,
                        new Rectangle(roadCenterX - halfRoadWidth + yellowInset, screenRow, sideLineWidth, 1),
                        YellowLineColor);
                    _spriteBatch.Draw(
                        _pixel,
                        new Rectangle(roadCenterX + halfRoadWidth - yellowInset - sideLineWidth, screenRow, sideLineWidth, 1),
                        YellowLineColor);
                }
            }
        }

        int carWidth = Math.Max((int)(viewport.Width * CarWidthFraction), 1);
        int carHeight = Math.Max((int)(viewport.Height * CarHeightFraction), 1);
        int carBottomMargin = (int)(viewport.Height * CarBottomMarginFraction);
        _spriteBatch.Draw(
            _pixel,
            new Rectangle(roadCenterX - carWidth / 2, viewport.Height - carHeight - carBottomMargin, carWidth, carHeight),
            CarColor);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
