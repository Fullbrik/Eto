using Eto.Drawing;
using sd = System.Drawing;
using MonoMac.Foundation;
#if Mac64
using CGFloat = System.Double;
using NSInteger = System.Int64;
using NSUInteger = System.UInt64;
#else
using NSSize = System.Drawing.SizeF;
using NSRect = System.Drawing.RectangleF;
using NSPoint = System.Drawing.PointF;
using CGFloat = System.Single;
using NSInteger = System.Int32;
using NSUInteger = System.UInt32;
#endif

#if OSX
using MonoMac.CoreGraphics;

namespace Eto.Mac.Drawing
#else
using MonoTouch.CoreGraphics;
using MonoTouch.ImageIO;

namespace Eto.iOS.Drawing
#endif
{
	/// <summary>
	/// Handler for <see cref="TextureBrush"/>
	/// </summary>
	/// <copyright>(c) 2012-2014 by Curtis Wensley</copyright>
	/// <license type="BSD-3">See LICENSE for full terms</license>
	public class TextureBrushHandler : BrushHandler, TextureBrush.IHandler
	{
		class BrushObject
		{
			CGImage image;
			CGAffineTransform transform = CGAffineTransform.MakeIdentity();
			CGAffineTransform viewTransform = CGAffineTransform.MakeIdentity();
			readonly CGFloat[] alpha = { 1 };
			CGPattern pattern;

			public void Apply(GraphicsHandler graphics)
			{
				graphics.SetFillColorSpace();

				#if OSX
				if (graphics.DisplayView != null)
				{
					// adjust for position of the current view relative to the window
					var pos = graphics.DisplayView.ConvertPointToView(NSPoint.Empty, null);
					graphics.Control.SetPatternPhase(new NSSize(pos.X, pos.Y));
				}
				#endif

				// make current transform apply to the pattern
				var currentTransform = graphics.CurrentTransform;
				if (pattern == null || viewTransform != currentTransform)
				{
					viewTransform = currentTransform;
					SetPattern();
				}

				graphics.Control.SetFillPattern(pattern, alpha);
			}

			public CGImage Image
			{
				get { return image; }
				set
				{
					image = value;
					ClearPattern();
				}
			}

			public float Opacity
			{
				get { return (float)alpha[0]; }
				set { alpha[0] = value; }
			}

			public CGAffineTransform Transform
			{
				get { return transform; }
				set
				{
					transform = value;					
					ClearPattern();
				}
			}

			void ClearPattern()
			{
				if (pattern != null)
					pattern.Dispose();
				pattern = null;
			}

			void DrawPattern(CGContext context)
			{
				var destRect = new NSRect(0, 0, image.Width, image.Height);
				context.ConcatCTM(new CGAffineTransform(1, 0, 0, -1, 0, image.Height));
				context.DrawImage(destRect, image);
			}

			void SetPattern()
			{
				var t = CGAffineTransform.Multiply(transform, viewTransform);
				ClearPattern();
				pattern = new CGPattern(new NSRect(0, 0, image.Width, image.Height), t, image.Width, image.Height, CGPatternTiling.NoDistortion, true, DrawPattern);
			}
		}

		public object Create(Image image, float opacity)
		{
			return new BrushObject
			{
				Image = image.ToCG(),
				Opacity = opacity
			};
		}

		public IMatrix GetTransform(TextureBrush widget)
		{
			return ((BrushObject)widget.ControlObject).Transform.ToEto();
		}

		public void SetTransform(TextureBrush widget, IMatrix transform)
		{
			((BrushObject)widget.ControlObject).Transform = transform.ToCG();
		}

		public override void Apply(object control, GraphicsHandler graphics)
		{
			((BrushObject)control).Apply(graphics);
		}

		public float GetOpacity(TextureBrush widget)
		{
			return ((BrushObject)widget.ControlObject).Opacity;
		}

		public void SetOpacity(TextureBrush widget, float opacity)
		{
			((BrushObject)widget.ControlObject).Opacity = opacity;
		}
	}
}

