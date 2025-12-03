using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using fire_and_ice.Interfaces;

namespace fire_and_ice.Systems
{
    /// <summary>
    /// Manages rendering of all game objects with proper ordering and sprite batch management.
    /// Handles sprite batch begin/end, render order sorting, and supports camera transformations.
    /// </summary>
    public class RenderingSystem
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private readonly List<IRenderable> _renderables;
        private bool _needsSorting;

        // Rendering configuration
        private SpriteSortMode _sortMode;
        private BlendState _blendState;
        private SamplerState _samplerState;
        private DepthStencilState _depthStencilState;
        private RasterizerState _rasterizerState;
        private Effect _effect;
        private Matrix? _transformMatrix;

        /// <summary>
        /// Gets or sets whether automatic sorting is enabled.
        /// When true, renderables are sorted by DrawOrder before each draw.
        /// </summary>
        public bool AutoSort { get; set; }

        /// <summary>
        /// Gets the number of registered renderables
        /// </summary>
        public int RenderableCount => _renderables.Count;

        /// <summary>
        /// Creates a new rendering system
        /// </summary>
        /// <param name="graphicsDevice">Graphics device for rendering</param>
        /// <param name="spriteBatch">Sprite batch for drawing 2D sprites</param>
        public RenderingSystem(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
            _renderables = new List<IRenderable>();
            _needsSorting = false;

            // Default rendering settings
            _sortMode = SpriteSortMode.Deferred;
            _blendState = BlendState.AlphaBlend;
            _samplerState = SamplerState.PointClamp; // Pixel-perfect rendering for retro games
            _depthStencilState = null;
            _rasterizerState = null;
            _effect = null;
            _transformMatrix = null;

            AutoSort = true;
        }

        /// <summary>
        /// Registers a renderable object to be drawn by this system
        /// </summary>
        /// <param name="renderable">The renderable object to register</param>
        public void Register(IRenderable renderable)
        {
            if (renderable == null)
                throw new ArgumentNullException(nameof(renderable));

            if (!_renderables.Contains(renderable))
            {
                _renderables.Add(renderable);
                _needsSorting = true;
            }
        }

        /// <summary>
        /// Unregisters a renderable object from this system
        /// </summary>
        /// <param name="renderable">The renderable object to unregister</param>
        public void Unregister(IRenderable renderable)
        {
            if (renderable != null)
            {
                _renderables.Remove(renderable);
            }
        }

        /// <summary>
        /// Clears all registered renderables
        /// </summary>
        public void Clear()
        {
            _renderables.Clear();
            _needsSorting = false;
        }

        /// <summary>
        /// Configures sprite batch rendering settings
        /// </summary>
        /// <param name="sortMode">Sprite sorting mode</param>
        /// <param name="blendState">Blend state (null for default)</param>
        /// <param name="samplerState">Sampler state (null for default)</param>
        /// <param name="depthStencilState">Depth stencil state (null for default)</param>
        /// <param name="rasterizerState">Rasterizer state (null for default)</param>
        /// <param name="effect">Effect to apply (null for none)</param>
        /// <param name="transformMatrix">Transformation matrix (null for none, useful for camera)</param>
        public void Configure(
            SpriteSortMode sortMode = SpriteSortMode.Deferred,
            BlendState blendState = null,
            SamplerState samplerState = null,
            DepthStencilState depthStencilState = null,
            RasterizerState rasterizerState = null,
            Effect effect = null,
            Matrix? transformMatrix = null)
        {
            _sortMode = sortMode;
            _blendState = blendState ?? BlendState.AlphaBlend;
            _samplerState = samplerState ?? SamplerState.PointClamp;
            _depthStencilState = depthStencilState;
            _rasterizerState = rasterizerState;
            _effect = effect;
            _transformMatrix = transformMatrix;
        }

        /// <summary>
        /// Sets the camera transformation matrix for rendering
        /// </summary>
        /// <param name="transformMatrix">Camera transformation matrix (null to disable camera)</param>
        public void SetCameraTransform(Matrix? transformMatrix)
        {
            _transformMatrix = transformMatrix;
        }

        /// <summary>
        /// Manually triggers sorting of renderables by DrawOrder.
        /// Called automatically before Draw() if AutoSort is true.
        /// </summary>
        public void Sort()
        {
            _renderables.Sort((a, b) => a.DrawOrder.CompareTo(b.DrawOrder));
            _needsSorting = false;
        }

        /// <summary>
        /// Draws all registered renderables in order.
        /// Automatically handles SpriteBatch.Begin() and End().
        /// </summary>
        public void Draw()
        {
            // Sort if needed
            if (AutoSort && _needsSorting)
            {
                Sort();
            }

            // Begin sprite batch with configured settings
            _spriteBatch.Begin(
                sortMode: _sortMode,
                blendState: _blendState,
                samplerState: _samplerState,
                depthStencilState: _depthStencilState,
                rasterizerState: _rasterizerState,
                effect: _effect,
                transformMatrix: _transformMatrix);

            // Draw all renderables
            foreach (var renderable in _renderables)
            {
                if (renderable != null)
                {
                    renderable.Draw(_spriteBatch);
                }
            }

            // End sprite batch
            _spriteBatch.End();
        }

        /// <summary>
        /// Draws only renderables within a specified DrawOrder range.
        /// Useful for drawing specific layers (background, entities, UI, etc.)
        /// </summary>
        /// <param name="minOrder">Minimum DrawOrder (inclusive)</param>
        /// <param name="maxOrder">Maximum DrawOrder (inclusive)</param>
        public void DrawLayer(int minOrder, int maxOrder)
        {
            // Sort if needed
            if (AutoSort && _needsSorting)
            {
                Sort();
            }

            // Begin sprite batch
            _spriteBatch.Begin(
                sortMode: _sortMode,
                blendState: _blendState,
                samplerState: _samplerState,
                depthStencilState: _depthStencilState,
                rasterizerState: _rasterizerState,
                effect: _effect,
                transformMatrix: _transformMatrix);

            // Draw renderables in the specified range
            foreach (var renderable in _renderables)
            {
                if (renderable != null &&
                    renderable.DrawOrder >= minOrder &&
                    renderable.DrawOrder <= maxOrder)
                {
                    renderable.Draw(_spriteBatch);
                }
            }

            // End sprite batch
            _spriteBatch.End();
        }

        /// <summary>
        /// Draws specific renderables without managing sprite batch.
        /// Assumes SpriteBatch.Begin() has already been called.
        /// Use this when you need manual control over sprite batch lifecycle.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        public void DrawWithExternalBatch(SpriteBatch spriteBatch)
        {
            // Sort if needed
            if (AutoSort && _needsSorting)
            {
                Sort();
            }

            // Draw all renderables using external sprite batch
            foreach (var renderable in _renderables)
            {
                if (renderable != null)
                {
                    renderable.Draw(spriteBatch);
                }
            }
        }

        /// <summary>
        /// Gets all renderables in a specific DrawOrder range
        /// </summary>
        /// <param name="minOrder">Minimum DrawOrder (inclusive)</param>
        /// <param name="maxOrder">Maximum DrawOrder (inclusive)</param>
        /// <returns>List of renderables in the specified range</returns>
        public List<IRenderable> GetRenderablesInRange(int minOrder, int maxOrder)
        {
            return _renderables
                .Where(r => r != null && r.DrawOrder >= minOrder && r.DrawOrder <= maxOrder)
                .ToList();
        }
    }
}
