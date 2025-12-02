using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace fire_and_ice
{
    public class ImageLevelBackground
    {
        //attributes for background image
        private GraphicsDevice _graphicsDevice;
        private Texture2D _backgroundTexture;
        private int _screenWidth;
        private int _screenHeight;

        public int LevelHeight
        {
            get 
            { 
            if(_backgroundTexture != null)
                    return _backgroundTexture.Height;
            else 
                    return _screenHeight;
            
            }
        
        }

        public int LevelWidth
        {
            get
            {
                if (_backgroundTexture != null)
                    return _backgroundTexture.Width;
                else
                    return _screenWidth;
            }
        }
        //stolen from tutorial idk how it works dont delete tho 
        public ImageLevelBackground(GraphicsDevice graphicsDevice, ContentManager content, bool enableScrolling = false)
        {
            _graphicsDevice = graphicsDevice;
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;

            // Load the background image
            _backgroundTexture = content.Load<Texture2D>("first_level");

        }
        
       
    }
}