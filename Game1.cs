using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace DepthBasics_KinectMonogame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

		/// <summary>
		/// Active Kinect sensor
		/// </summary>
		private KinectSensor sensor;

		/// <summary>
		/// Intermediate storage for the depth data received from the camera
		/// </summary>
		private DepthImagePixel[] depthPixels;

		/// <summary>
		/// the texture to write to
		/// </summary>
		Texture2D pixels;

		/// <summary>
		/// temp buffer to hold convert kinect data to color objects
		/// </summary>
		Color[] pixelData_clear;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

			pixels = new Texture2D(graphics.GraphicsDevice,
							640,
							480, false, SurfaceFormat.Color);
			pixelData_clear = new Color[640 * 480];
			for (int i = 0; i < pixelData_clear.Length; ++i)
				pixelData_clear[i] = Color.Black;

			// Look through all sensors and start the first connected one.
			// This requires that a Kinect is connected at the time of app startup.
			// To make your app robust against plug/unplug, 
			// it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
			foreach (var potentialSensor in KinectSensor.KinectSensors)
			{
				if (potentialSensor.Status == KinectStatus.Connected)
				{
					this.sensor = potentialSensor;
					break;
				}
			}

			if (null != this.sensor)
			{
				// Turn on the depth stream to receive depth frames
				this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

				// Allocate space to put the depth pixels we'll receive
				this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];

				// Add an event handler to be called whenever there is new depth frame data
				this.sensor.DepthFrameReady += this.SensorDepthFrameReady;

				sensor.SkeletonStream.Enable();

				// Start the sensor!
				try
				{
					this.sensor.Start();
					//sensor.ColorStream.CameraSettings.BacklightCompensationMode = BacklightCompensationMode.CenterOnly;
				}
				catch (IOException)
				{
					this.sensor = null;
				}
			}

			//if (null == this.sensor)
			//{
			//	this.statusBarText.Text = Properties.Resources.NoKinectReady;
			//}
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
			if (null != this.sensor)
			{
				this.sensor.Stop();
			}
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

			pixels.SetData<Color>(pixelData_clear);
			spriteBatch.Begin();
			spriteBatch.Draw(pixels, new Vector2(0, 0), null, Color.White);
			spriteBatch.End();

            base.Draw(gameTime);
        }

		/// <summary>
		/// Event handler for Kinect sensor's DepthFrameReady event
		/// </summary>
		/// <param name="sender">object sending the event</param>
		/// <param name="e">event arguments</param>
		private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
		{
			using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
			{
				if (depthFrame != null)
				{
					// Copy the pixel data from the image to a temporary array
					depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

					// Get the min and max reliable depth for the current frame
					int minDepth = depthFrame.MinDepth;
					int maxDepth = depthFrame.MaxDepth;

					//Get the depth delta
					int depthDelta = maxDepth - minDepth;

					// Convert the depth to RGB
					for (int depthIndex = 0; depthIndex < depthPixels.Length; depthIndex++)
					{
						// Get the depth for this pixel
						short depth = depthPixels[depthIndex].Depth;

						//convert to a range that will fit in one byte
						byte intensity = 0;
						if (depth >= minDepth && depth <= maxDepth)
						{
							intensity = (byte)((depth * byte.MaxValue) / depthDelta);
						}

						//set the color
						pixelData_clear[depthIndex].R = intensity;
						pixelData_clear[depthIndex].G = intensity;
						pixelData_clear[depthIndex].B = intensity;
					}
				}
			}
		}
    }
}
