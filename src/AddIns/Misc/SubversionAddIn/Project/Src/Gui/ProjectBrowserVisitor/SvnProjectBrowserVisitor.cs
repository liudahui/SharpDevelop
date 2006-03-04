// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.IO;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.Svn
{
	/// <summary>
	/// Description of SvnProjectBrowserVisitor.
	/// </summary>
	public class SvnProjectBrowserVisitor : ProjectBrowserTreeNodeVisitor
	{
		public override object Visit(SolutionNode node, object data)
		{
			if (Directory.Exists(Path.Combine(node.Solution.Directory, ".svn"))) {
				OverlayIconManager.Enqueue(node);
			}
			return node.AcceptChildren(this, data);
		}
		
		public override object Visit(ProjectNode node, object data)
		{
			return Visit((DirectoryNode)node, data);
		}
		
		public override object Visit(DirectoryNode node, object data)
		{
			if (Directory.Exists(Path.Combine(node.Directory, ".svn"))) {
				OverlayIconManager.Enqueue(node);
				return node.AcceptChildren(this, data);
			}
			return data;
		}
		
		public override object Visit(FileNode node, object data)
		{
			OverlayIconManager.Enqueue(node);
			return node.AcceptChildren(this, data);
		}
	}
}
