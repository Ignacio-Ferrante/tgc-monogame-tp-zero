using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Content.Models;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Clase principal del juego.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";
        
        private GraphicsDeviceManager Graphics { get; }
        private CityScene City { get; set; }
        private Model CarModel { get; set; }
        private Effect CarEffect { get; set; }
        private Matrix CarWorld { get; set; }
        private FollowCamera FollowCamera { get; set; }

        private Matrix CarRotation { get; set; }
        private Vector3 CarPosition { get; set; }
        private Vector3 CarVelocity { get; set; }
        private Vector3 CarAcceleration { get; set; }
        private Vector3 CarDirection { get; set; }


        /// <summary>
        ///     Constructor del juego
        /// </summary>
        public TGCGame()
        {
            // Se encarga de la configuracion y administracion del Graphics Device
            Graphics = new GraphicsDeviceManager(this);

            // Carpeta donde estan los recursos que vamos a usar
            Content.RootDirectory = "Content";

            // Hace que el mouse sea visible
            IsMouseVisible = true;
        }

        /// <summary>
        ///     Llamada una vez en la inicializacion de la aplicacion.
        ///     Escribir aca todo el codigo de inicializacion: Todo lo que debe estar precalculado para la aplicacion.
        /// </summary>
        protected override void Initialize()
        {
            // Enciendo Back-Face culling
            // Configuro Blend State a Opaco
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            GraphicsDevice.RasterizerState = rasterizerState;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // Configuro el tamaño de la pantalla
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            Graphics.ApplyChanges();

            // Configuro la matriz de mundo del auto
            CarWorld = Matrix.Identity;

            // Creo una camara para seguir a nuestro auto
            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);

            
            //Inicializo la posicion del auto
            CarPosition = Vector3.Zero;
            CarRotation = Matrix.Identity;
            CarDirection = Vector3.Backward;

            // Set the Acceleration (which in this case won't change) to the Gravity pointing down
            CarAcceleration = Vector3.Down * 350f;
            // Initialize the Velocity as zero
            CarVelocity = Vector3.Zero;

            base.Initialize();
        }

        /// <summary>
        ///     Llamada una sola vez durante la inicializacion de la aplicacion, luego de Initialize, y una vez que fue configurado GraphicsDevice.
        ///     Debe ser usada para cargar los recursos y otros elementos del contenido.
        /// </summary>
        protected override void LoadContent()
        {
            // Creo la escena de la ciudad
            City = new CityScene(Content);

            // La carga de contenido debe ser realizada aca

            CarModel = Content.Load<Model>(ContentFolder3D + "scene/car");
            CarEffect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");

            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.
            foreach (var mesh in CarModel.Meshes)
                // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
                foreach (var meshPart in mesh.MeshParts)
                    meshPart.Effect = CarEffect;

            base.LoadContent();
        }

        /// <summary>
        ///     Es llamada N veces por segundo. Generalmente 60 veces pero puede ser configurado.
        ///     La logica general debe ser escrita aca, junto al procesamiento de mouse/teclas
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Caputo el estado del teclado
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
                // Salgo del juego
                Exit();

            // La logica debe ir aca

            float unidad = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            if (Keyboard.GetState().IsKeyDown(Keys.A))
                CarRotation *= Matrix.CreateRotationY(unidad * 3f);

            if (Keyboard.GetState().IsKeyDown(Keys.D))
                CarRotation *= Matrix.CreateRotationY(-unidad * 3f);

            if (Keyboard.GetState().IsKeyDown(Keys.W))
                CarPosition += Vector3.Transform(Vector3.Forward, CarRotation) * unidad * 500f;

            if (Keyboard.GetState().IsKeyDown(Keys.S))
                CarPosition += Vector3.Transform(Vector3.Backward, CarRotation) * unidad * 500f;

            // Actualizo la camara, enviandole la matriz de mundo del auto
            if (keyboardState.IsKeyDown(Keys.X))
                FollowCamera.Update(gameTime, CarWorld);

            base.Update(gameTime);
        }


        /// <summary>
        ///     Llamada para cada frame
        ///     La logica de dibujo debe ir aca.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Limpio la pantalla
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Dibujo la ciudad
            City.Draw(gameTime, FollowCamera.View, FollowCamera.Projection);

            var traslacionMatrix = Matrix.CreateTranslation(CarPosition);

            // El dibujo del auto debe ir aca

            foreach (var mesh in CarModel.Meshes)
            {
                CarWorld = mesh.ParentBone.Transform * CarRotation * traslacionMatrix;
                CarEffect.Parameters["World"].SetValue(CarWorld);
                mesh.Draw();
            }

            base.Draw(gameTime);
        }

        /// <summary>
        ///     Libero los recursos cargados
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos cargados dessde Content Manager
            Content.Unload();

            base.UnloadContent();
        }
    }
}