﻿// <file>
//     <copyright see="prj:///doc/copyright.txt">2002-2005 AlphaSierraPapa</copyright>
//     <license see="prj:///doc/license.txt">GNU General Public License</license>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Resources;
using System.Xml;
using System.Threading;
using System.Runtime.Remoting;
using System.Security.Policy;

using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Commands;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop
{
	/// <summary>
	/// This Class is the Core main class, it starts the program.
	/// </summary>
	public class SharpDevelopMain
	{
		static string[] commandLineArgs = null;
		
		public static string[] CommandLineArgs {
			get {
				return commandLineArgs;
			}
		}
		
		static void ShowErrorBox(object sender, ThreadExceptionEventArgs e)
		{
			LoggingService.Error("ThreadException caught", e.Exception);
			ShowErrorBox(e.Exception, null);
		}
		
		static void ShowErrorBox(object sender, UnhandledExceptionEventArgs e)
		{
			Exception ex = e.ExceptionObject as Exception;
			LoggingService.Fatal("UnhandledException caught", ex);
			if (e.IsTerminating)
				LoggingService.Fatal("Runtime is terminating because of unhandled exception.");
			ShowErrorBox(ex, "Unhandled exception", e.IsTerminating);
		}
		
		static void ShowErrorBox(Exception exception, string message)
		{
			ShowErrorBox(exception, message, false);
		}
		
		static void ShowErrorBox(Exception exception, string message, bool mustTerminate)
		{
			try {
				using (ExceptionBox box = new ExceptionBox(exception, message, mustTerminate)) {
					try {
						box.ShowDialog(ICSharpCode.SharpDevelop.Gui.WorkbenchSingleton.MainForm);
					} catch (InvalidOperationException) {
						box.ShowDialog();
					}
				}
			} catch (Exception ex) {
				LoggingService.Warn("Error showing ExceptionBox", ex);
				MessageBox.Show(exception.ToString());
			}
		}
		
		/// <summary>
		/// Starts the core of SharpDevelop.
		/// </summary>
		[STAThread()]
		public static void Main(string[] args)
		{
			#if DEBUG
			if (Debugger.IsAttached) {
				Run(args);
				return;
			}
			#endif
			// Do not use LoggingService here (see comment in Run(string[]))
			try {
				Run(args);
			} catch (Exception ex) {
				HandleMainException(ex);
			}
		}
		
		static void HandleMainException(Exception ex)
		{
			LoggingService.Fatal(ex);
			try {
				Application.Run(new ExceptionBox(ex, "Unhandled exception terminated SharpDevelop", true));
			} catch {
				MessageBox.Show(ex.ToString(), "Critical error (cannot use ExceptionBox)");
			}
		}
		
		static void Run(string[] args)
		{
			// DO NOT USE LoggingService HERE!
			// LoggingService requires ICSharpCode.Core.dll and log4net.dll
			// When a method containing a call to LoggingService is JITted, the
			// libraries are loaded.
			// We want to show the SplashScreen while those libraries are loaded, so
			// don't call LoggingService.
			
			#if DEBUG
			Control.CheckForIllegalCrossThreadCalls = true;
			#endif
			commandLineArgs = args;
			bool noLogo = false;
			
			SplashScreenForm.SetCommandLineArgs(args);
			
			foreach (string parameter in SplashScreenForm.GetParameterList()) {
				switch (parameter.ToUpper()) {
					case "NOLOGO":
						noLogo = true;
						break;
				}
			}
			
			if (!noLogo) {
				SplashScreenForm.SplashScreen.Show();
			}
			try {
				RunApplication();
			} finally {
				if (SplashScreenForm.SplashScreen != null) {
					SplashScreenForm.SplashScreen.Dispose();
				}
			}
		}
		
		static void RunApplication()
		{
			LoggingService.Info("Starting SharpDevelop...");
			try {
				#if DEBUG
				if (!Debugger.IsAttached) {
					Application.ThreadException += ShowErrorBox;
					AppDomain.CurrentDomain.UnhandledException += ShowErrorBox;
				}
				#else
				MessageService.CustomErrorReporter = ShowErrorBox;
				#endif
				
				RegisterDoozers();
				
				InitializeCore();
				
				// finally start the workbench.
				try {
					LoggingService.Info("Starting workbench...");
					new StartWorkbenchCommand().Run(SplashScreenForm.GetRequestedFileList());
				} finally {
					LoggingService.Info("Unloading services...");
					ProjectService.CloseSolution();
					FileService.Unload();
					PropertyService.Save();
				}
			} finally {
				LoggingService.Info("Leaving RunApplication()");
			}
		}
		
		static void RegisterDoozers()
		{
			AddInTree.ConditionEvaluators.Add("ActiveContentExtension", new ActiveContentExtensionConditionEvaluator());
			AddInTree.ConditionEvaluators.Add("ActiveViewContentUntitled", new ActiveViewContentUntitledConditionEvaluator());
			AddInTree.ConditionEvaluators.Add("ActiveWindowState", new ActiveWindowStateConditionEvaluator());
			AddInTree.ConditionEvaluators.Add("CombineOpen", new CombineOpenConditionEvaluator());
			AddInTree.ConditionEvaluators.Add("DebuggerSupports", new DebuggerSupportsConditionEvaluator());
			AddInTree.ConditionEvaluators.Add("IsProcessRunning", new IsProcessRunningConditionEvaluator());
			AddInTree.ConditionEvaluators.Add("OpenWindowState", new OpenWindowStateConditionEvaluator());
			AddInTree.ConditionEvaluators.Add("WindowActive", new WindowActiveConditionEvaluator());
			AddInTree.ConditionEvaluators.Add("WindowOpen", new WindowOpenConditionEvaluator());
			AddInTree.ConditionEvaluators.Add("ProjectActive", new ProjectActiveConditionEvaluator());
			AddInTree.ConditionEvaluators.Add("ProjectOpen", new ProjectOpenConditionEvaluator());
			AddInTree.ConditionEvaluators.Add("TextContent", new ICSharpCode.SharpDevelop.DefaultEditor.Conditions.TextContentConditionEvaluator());
			AddInTree.ConditionEvaluators.Add("BrowserLocation", new ICSharpCode.SharpDevelop.BrowserDisplayBinding.BrowserLocationConditionEvaluator());
			
			AddInTree.Doozers.Add("DialogPanel", new DialogPanelDoozer());
			AddInTree.Doozers.Add("DisplayBinding", new DisplayBindingDoozer());
			AddInTree.Doozers.Add("Pad", new PadDoozer());
			AddInTree.Doozers.Add("LanguageBinding", new LanguageBindingDoozer());
			AddInTree.Doozers.Add("Parser", new ParserDoozer());
			AddInTree.Doozers.Add("EditAction", new ICSharpCode.SharpDevelop.DefaultEditor.Codons.EditActionDoozer());
			AddInTree.Doozers.Add("SyntaxMode", new ICSharpCode.SharpDevelop.DefaultEditor.Codons.SyntaxModeDoozer());
			AddInTree.Doozers.Add("BrowserSchemeExtension", new ICSharpCode.SharpDevelop.BrowserDisplayBinding.SchemeExtensionDoozer());
			AddInTree.Doozers.Add("CodeCompletionBinding", new ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor.CodeCompletionBindingDoozer());
			AddInTree.Doozers.Add("Debugger", new DebuggerDoozer());
			
			MenuCommand.LinkCommandCreator = delegate(string link) { return new LinkCommand(link); };
		}
		
		static void InitializeCore()
		{
			LoggingService.Info("Loading properties...");
			PropertyService.Load();
			
			StringParser.RegisterStringTagProvider(new SharpDevelopStringTagProvider());
			
			LoggingService.Info("Loading AddInTree...");
			AddInTree.Load();
			
			LoggingService.Info("Initializing workbench...");
			// .NET base autostarts
			// taken out of the add-in tree for performance reasons (every tick in startup counts)
			new InitializeWorkbenchCommand().Run();
			
			// run workspace autostart commands
			try {
				LoggingService.Info("Running autostart commands...");
				foreach (ICommand command in AddInTree.BuildItems("/Workspace/Autostart", null, false)) {
					command.Run();
				}
			} catch (XmlException e) {
				LoggingService.Error("Could not load XML", e);
				MessageBox.Show("Could not load XML :" + Environment.NewLine + e.Message);
				return;
			} finally {
				if (SplashScreenForm.SplashScreen != null) {
					SplashScreenForm.SplashScreen.Dispose();
				}
			}
		}
	}
}
