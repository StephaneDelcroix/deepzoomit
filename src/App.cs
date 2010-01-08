/*
 * DeepzoomIt.App.cs
 *
 * Author(s):
 *	Stephane Delcroix  <stephane@delcroix.org>
 *
 * Copyright (c) 2020 Stephane Delcroix  <stephane@delcroix.org>
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
 *
 */

using System;

namespace DeepzoomIt
{
	public class App
	{
		static int Main (string[] args)
		{
			if (args.Length < 1)
			{
				Console.Error.WriteLine ("you need to specify an image as argument");
				return -1;
			}

			try {
				using (var image = new Image (args[0])) {
					image.Process ();
					image.WriteFiles ();
					image.WriteManifest ();
				}
			} catch (Exception e) {
				Console.Error.WriteLine ("Something weird or unexpected happened:");
				Console.WriteLine (e);
				return -1;
			}
			Console.WriteLine ("DONE");
			return 0;
		}
	}
}
