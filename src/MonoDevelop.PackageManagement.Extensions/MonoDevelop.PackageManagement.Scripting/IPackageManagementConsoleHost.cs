﻿// 
// IPackageManagementConsoleHost.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2011-2014 Matthew Ward
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
using System.Collections.Generic;
using System.Threading;
using ICSharpCode.Scripting;
using MonoDevelop.PackageManagement;
using MonoDevelop.Projects;
using NuGet.Configuration;
using NuGet.ProjectManagement;
using NuGet.PackageManagement;
using NuGet.Protocol.Core.Types;

namespace ICSharpCode.PackageManagement.Scripting
{
	internal interface IPackageManagementConsoleHost : IDisposable
	{
		Project DefaultProject { get; set; }
		SourceRepositoryViewModel ActivePackageSource { get; set; }
		IScriptingConsole ScriptingConsole { get; set; }
		ISolutionManager SolutionManager { get; }
		ISettings Settings { get; }
		bool IsRunning { get; } 
		bool IsSolutionOpen { get; }

		CancellationToken Token { get; }
		void Clear ();
		void WritePrompt ();
		void Run ();
		void ShutdownConsole ();
		void ExecuteCommand (string command);
		void ProcessUserInput (string line);

		void SetDefaultRunspace ();

		IConsoleHostFileConflictResolver CreateFileConflictResolver (FileConflictAction fileConflictAction);
		IDisposable CreateEventsMonitor (NuGet.ILogger logger);

		string GetActivePackageSource (string source);

		IEnumerable<NuGetProject> GetNuGetProjects ();
		NuGetProject GetNuGetProject (string projectName);

		IEnumerable<PackageSource> LoadPackageSources ();
		SourceRepository CreateRepository (PackageSource source);
		IEnumerable<SourceRepository> GetRepositories ();
	}
}
