﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)
using System;
using System.Windows.Controls;
using NUnit.Framework;

namespace ICSharpCode.WpfDesign.Tests.Designer
{
	[TestFixture]
	public class NamespaceTests : ModelTestHelper
	{
	
		[Test]
		public void AddControlFromTestNamespace()
		{
			DesignItem button = CreateCanvasContext("<Button />");
			
			DesignItem canvas = button.Parent;
			
			DesignItem customButton = canvas.Services.Component.RegisterComponentForDesigner(new CustomButton());
			canvas.Properties["Children"].CollectionElements.Add(customButton);

			AssertCanvasDesignerOutput("<Button />\n" +
			                           "<t:CustomButton />", canvas.Context);
		}
		
		[Test]
		public void AddControlWithUndeclaredNamespace()
		{
			DesignItem button = CreateCanvasContext("<Button />");
			
			DesignItem canvas = button.Parent;
			
			DesignItem customButton = canvas.Services.Component.RegisterComponentForDesigner(new ICSharpCode.WpfDesign.Tests.OtherControls.CustomButton());
			canvas.Properties["Children"].CollectionElements.Add(customButton);

			AssertCanvasDesignerOutput("<Button />\n" +
			                           "<Controls0:CustomButton />",
			                           canvas.Context,
			                           "xmlns:Controls0=\"clr-namespace:ICSharpCode.WpfDesign.Tests.OtherControls;assembly=ICSharpCode.WpfDesign.Tests\"");
		}
		
		[Test]
		public void AddControlWithUndeclaredNamespaceThatUsesXmlnsPrefixAttribute()
		{
			DesignItem button = CreateCanvasContext("<Button />");
			
			DesignItem canvas = button.Parent;
			
			DesignItem customButton = canvas.Services.Component.RegisterComponentForDesigner(new ICSharpCode.WpfDesign.Tests.Controls.CustomButton());
			canvas.Properties["Children"].CollectionElements.Add(customButton);

			AssertCanvasDesignerOutput("<Button />\n" +
			                           "<sdtcontrols:CustomButton />",
			                           canvas.Context,
			                           "xmlns:sdtcontrols=\"http://sharpdevelop.net/WpfDesign/Tests/Controls\"");
		}
		
		[Test]
		public void AddMultipleControls()
		{
			DesignItem button = CreateCanvasContext("<Button />");
			
			DesignItem canvas = button.Parent;
			
			DesignItem customControl = canvas.Services.Component.RegisterComponentForDesigner(new CustomButton());
			canvas.Properties["Children"].CollectionElements.Add(customControl);
			
			customControl = canvas.Services.Component.RegisterComponentForDesigner(new ICSharpCode.WpfDesign.Tests.Controls.CustomButton());
			canvas.Properties["Children"].CollectionElements.Add(customControl);
			
			customControl = canvas.Services.Component.RegisterComponentForDesigner(new ICSharpCode.WpfDesign.Tests.OtherControls.CustomButton());
			canvas.Properties["Children"].CollectionElements.Add(customControl);
			
			customControl = canvas.Services.Component.RegisterComponentForDesigner(new ICSharpCode.WpfDesign.Tests.SpecialControls.CustomButton());
			canvas.Properties["Children"].CollectionElements.Add(customControl);
			
			customControl = canvas.Services.Component.RegisterComponentForDesigner(new CustomCheckBox());
			canvas.Properties["Children"].CollectionElements.Add(customControl);
			
			customControl = canvas.Services.Component.RegisterComponentForDesigner(new ICSharpCode.WpfDesign.Tests.Controls.CustomCheckBox());
			canvas.Properties["Children"].CollectionElements.Add(customControl);
			
			customControl = canvas.Services.Component.RegisterComponentForDesigner(new ICSharpCode.WpfDesign.Tests.OtherControls.CustomCheckBox());
			canvas.Properties["Children"].CollectionElements.Add(customControl);
			
			customControl = canvas.Services.Component.RegisterComponentForDesigner(new ICSharpCode.WpfDesign.Tests.SpecialControls.CustomCheckBox());
			canvas.Properties["Children"].CollectionElements.Add(customControl);

			AssertCanvasDesignerOutput("<Button />\n" +
			                           "<t:CustomButton />\n" +
			                           "<sdtcontrols:CustomButton />\n" +
			                           "<Controls0:CustomButton />\n" +
			                           "<Controls1:CustomButton />\n" +
			                           "<t:CustomCheckBox />\n" +
			                           "<sdtcontrols:CustomCheckBox />\n" +
			                           "<Controls0:CustomCheckBox />\n" +
			                           "<Controls1:CustomCheckBox />",
			                           canvas.Context,
			                           "xmlns:sdtcontrols=\"http://sharpdevelop.net/WpfDesign/Tests/Controls\"",
			                           "xmlns:Controls0=\"clr-namespace:ICSharpCode.WpfDesign.Tests.OtherControls;assembly=ICSharpCode.WpfDesign.Tests\"",
			                           "xmlns:Controls1=\"clr-namespace:ICSharpCode.WpfDesign.Tests.SpecialControls;assembly=ICSharpCode.WpfDesign.Tests\"");
		}
	}
	
	public class CustomButton : Button
	{
	}
	
	public class CustomCheckBox : CheckBox
	{
	}
}

namespace ICSharpCode.WpfDesign.Tests.Controls
{
	public class CustomButton : Button
	{
	}
	
	public class CustomCheckBox : CheckBox
	{
	}
}

namespace ICSharpCode.WpfDesign.Tests.OtherControls
{
	public class CustomButton : Button
	{
	}
	
	public class CustomCheckBox : CheckBox
	{
	}
}

namespace ICSharpCode.WpfDesign.Tests.SpecialControls
{
	public class CustomButton : Button
	{
	}
	
	public class CustomCheckBox : CheckBox
	{
	}
}