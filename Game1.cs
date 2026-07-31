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
    private const float HorizonRatio = 0.4f;

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

        GraphicsDevice.Clear(SkyColor);

        _spriteBatch.Begin();
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, viewport.Width, horizonY), SkyColor);
        _spriteBatch.Draw(_pixel, new Rectangle(0, horizonY, viewport.Width, viewport.Height - horizonY), GrassColor);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
