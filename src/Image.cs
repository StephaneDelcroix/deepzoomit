/*
 * DeepzoomIt.Image.cs
 *
 * Author(s):
 *	Stephane Delcroix  <stephane@delcroix.org>
 *
 * Copyright (c) 2010 Stephane Delcroix  <stephane@delcroix.org>
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using System.IO;
using System.Collections.Generic;

using Gdk;

namespace DeepzoomIt
{
	public class Image : IDisposable
	{
#region pulbic API
		public Image (string image_path)
		{
			if (String.IsNullOrEmpty (image_path))
				throw new ArgumentException ("image_path is null or empty string");
			ImagePath = image_path;

			GLib.GType.Init ();
		}

		public void Process ()
		{
			Process (256);
		}
		public void Process (int tile_size)
		{
			if (tiles != null)
				throw new InvalidOperationException ("Only call Process () once");
			tiles = new Dictionary<TileKey, Pixbuf> ();
			Console.WriteLine ("Loading image...");
			using (var master = new Pixbuf (ImagePath)) {
				Width = master.Width;
				Height = master.Height;
				Console.WriteLine ("Image Size: {0}x{1}", Width, Height);
				int layers = (int)Math.Ceiling (Math.Log (Math.Max (Width, Height), 2));

				for (int l = layers; l >= 0; l--) {
					Console.Write ("Processing layer {0}", l);
					if (l == layers) {
						for (int i = 0; i * tile_size < Width; i++)
							for (int j = 0; j * tile_size < Height; j ++) {
								var key = new TileKey (l, i, j);
								var pb = new Pixbuf (master, i * tile_size, j*tile_size, Math.Min (tile_size, Width - i * tile_size), Math.Min (tile_size, Height - j *tile_size));
								tiles.Add (key, pb);
								Console.Write (".");
							}

					//All other layers are based on the upper one, to avoid huge (slow) rescaling
					//and a bug in gdk_pixbuf that doesn't allow scaling pb bigger than 65536px
					} else {
						tile_size *= 2;
						for (int i = 0; i * tile_size < Width; i++)
							for (int j = 0; j * tile_size < Height; j ++) {
								Pixbuf tl, tr, bl, br;
								tiles.TryGetValue (new TileKey (l+1, i*2, j*2),out tl);
								tiles.TryGetValue (new TileKey (l+1, i*2 + 1, j*2),out tr);
								tiles.TryGetValue (new TileKey (l+1, i*2, j*2 + 1),out bl);
								tiles.TryGetValue (new TileKey (l+1, i*2 + 1, j*2 +1),out br);
								var key = new TileKey (l, i, j);
								Pixbuf scaled;
								using (var pb = new Pixbuf (
										master.Colorspace,
										master.HasAlpha,
										master.BitsPerSample,
										tl.Width + (tr == null ? 0 : tr.Width),
										tl.Height + (bl == null ? 0 : bl.Height))) {
									tl.CopyArea (0, 0, tl.Width, tl.Height, pb, 0, 0);
									if (tr != null)
										tr.CopyArea (0, 0, tr.Width, tr.Height, pb, tl.Width, 0);
									if (bl != null)
										bl.CopyArea (0, 0, bl.Width, bl.Height, pb, 0, tl.Height);
									if (br != null)
										br.CopyArea (0, 0, br.Width, br.Height, pb, tl.Width, tl.Height);
									
									scaled = pb.ScaleSimple ((int)Math.Ceiling (pb.Width / 2.0), (int)Math.Ceiling (pb.Height / 2.0), InterpType.Bilinear);
								}
								tiles.Add (key, scaled);
								Console.Write(".");
							}
					}

					Console.WriteLine ();
				}
			}
		}

		public void WriteFiles ()
		{
			if (is_disposed)
				throw new InvalidOperationException ("can't call this after being disposed");
			if (tiles == null)
				throw new InvalidOperationException ("You must call Process () before this");
			var directory = Path.GetFileNameWithoutExtension (ImagePath)+"_files";
			if (!Directory.Exists (directory))
				Directory.CreateDirectory (directory);
			foreach (var tile in tiles) {
				if (!Directory.Exists (Path.Combine (directory, tile.Key.Level.ToString ())))
					Directory.CreateDirectory (Path.Combine (directory, tile.Key.Level.ToString ()));
				tile.Value.Save (Path.Combine (directory, Path.Combine (tile.Key.Level.ToString(), String.Format ("{1}_{2}.jpg", tile.Key.Level, tile.Key.X, tile.Key.Y))), "jpeg");
			}
		}

		public void WriteManifest ()
		{
		}
#endregion

#region IDisposable
		bool is_disposed;
		void IDisposable.Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		~Image ()
		{
			Dispose (false);
		}

		protected virtual void Dispose (bool is_disposing)
		{
			if (is_disposed)
				return;
			if (is_disposing) {
				//Free managed resources here
			}
			//Free unmanaged resources here
			if (tiles != null) {
				foreach (var pb in tiles.Values)
					pb.Dispose ();
			}
			is_disposed = true;
		}
#endregion
		string ImagePath {get; set;}
		int Width {get; set;}
		int Height {get; set;}
		Dictionary<TileKey, Pixbuf> tiles;
	}
}
