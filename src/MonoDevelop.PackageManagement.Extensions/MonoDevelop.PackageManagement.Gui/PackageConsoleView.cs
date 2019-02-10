﻿// 
// PackageConsoleView.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2014 Matthew Ward
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using Gdk;
using MonoDevelop.Components;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Ide.Fonts;
using MonoDevelop.PackageManagement.Scripting;

namespace MonoDevelop.PackageManagement
{
	public class PackageConsoleView : ConsoleView2, IScriptingConsole
	{
		const int DefaultMaxVisibleColumns = 160;
		int maxVisibleColumns = 0;
		int originalWidth = -1;

		public PackageConsoleView ()
		{
			// HACK - to allow text to appear before first prompt.
			PromptString = String.Empty;
			base.Clear ();
			
			SetFont (FontService.MonospaceFont);

			TextView.FocusInEvent += (o, args) => {
				TextViewFocused?.Invoke (this, args);
			};
		}

		public event EventHandler TextViewFocused;
		public event EventHandler MaxVisibleColumnsChanged;

		void WriteOutputLine (string message, ScriptingStyle style)
		{
			WriteOutput (message + Environment.NewLine, GetLogLevel (style));
		}
		
		void WriteOutputLine (string format, params object[] args)
		{
			WriteOutput (String.Format (format, args) + Environment.NewLine);
		}

		LogLevel GetLogLevel (ScriptingStyle style)
		{
			switch (style) {
			case ScriptingStyle.Error:
				return LogLevel.Error;
			case ScriptingStyle.Warning:
				return LogLevel.Warning;
			case ScriptingStyle.Debug:
				return LogLevel.Debug;
			default:
				return LogLevel.Default;
			}
		}
		
		public bool ScrollToEndWhenTextWritten { get; set; }
		
		public void SendLine (string line)
		{
		}
		
		public void SendText (string text)
		{
		}
		
		public void WriteLine ()
		{
			Runtime.RunInMainThread (() => {
				WriteOutput ("\n");
			}).Wait ();
		}
		
		public void WriteLine (string text, ScriptingStyle style)
		{
			Runtime.RunInMainThread (() => {
				if (style == ScriptingStyle.Prompt) {
					WriteOutputLine (text, style);
					ConfigurePromptString ();
					Prompt (true);
				} else {
					WriteOutputLine (text, style);
				}
			}).Wait ();
		}
		
		void ConfigurePromptString()
		{
			PromptString = "PM> ";
		}
		
		public void Write (string text, ScriptingStyle style)
		{
			Runtime.RunInMainThread (() => {
				if (style == ScriptingStyle.Prompt) {
					ConfigurePromptString ();
					Prompt (false);
				} else {
					WriteOutput (text);
				}
			}).Wait ();
		}
		
		public string ReadLine (int autoIndentSize)
		{
			throw new NotImplementedException();
		}
		
		public string ReadFirstUnreadLine ()
		{
			throw new NotImplementedException();
		}
		
		public int GetMaximumVisibleColumns ()
		{
			if (maxVisibleColumns > 0) {
				return maxVisibleColumns;
			}
			return DefaultMaxVisibleColumns;
		}

		void IScriptingConsole.Clear ()
		{
			Runtime.RunInMainThread (() => {
				base.ClearWithoutPrompt ();
			});
		}

		protected override void OnSizeAllocated (Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);

			int windowWidth = Allocation.Width;
			if (originalWidth == windowWidth) {
				return;
			}

			int originalMaxVisibleColumns = maxVisibleColumns;
			if (windowWidth > 0) {
				using (var layout = new Pango.Layout (PangoContext)) {
					layout.FontDescription = FontService.MonospaceFont;
					layout.SetText ("W");
					layout.GetSize (out int characterWidth, out int _);
					if (characterWidth > 0) {
						double characterPixelWidth = characterWidth / Pango.Scale.PangoScale;
						maxVisibleColumns = (int)(windowWidth / characterPixelWidth);
					} else {
						maxVisibleColumns = DefaultMaxVisibleColumns;
					}
				}
			} else {
				maxVisibleColumns = DefaultMaxVisibleColumns;
			}

			originalWidth = windowWidth;
			if (originalMaxVisibleColumns != maxVisibleColumns) {
				MaxVisibleColumnsChanged?.Invoke (this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Not sure why Copy command does not work with the keyboard shortcut. If the currently focused
		/// window is a TextView then it should work. The Immediate Pad does not have this problem and
		/// it does not have its own Copy command handler. It seems that the IdeApp.Workebench.RootWindow
		/// is the TextArea for the text editor not the pad.
		/// </summary>
		[CommandHandler (EditCommands.Copy)]
		void CopyText ()
		{
			// This is based on what the DefaultCopyCommandHandler does.
			var clipboard = Gtk.Clipboard.Get (Gdk.Atom.Intern ("CLIPBOARD", false));
			TextView.Buffer.CopyClipboard (clipboard);
		}
	}
}
