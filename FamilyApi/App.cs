#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Reflection;
#endregion

namespace FamilyApi
{
  class App : IExternalApplication
  {
    /// <summary>
    /// Add buttons for our three commands 
    /// to the ribbon panel.
    /// </summary>
      void PopulatePanel(RibbonPanel p)
      {
          string path = Assembly.GetExecutingAssembly().Location;
          /*SplitButtonData sb1 = new SplitButtonData("splitButton1", "Split");
          SplitButton sb = p.AddItem(sb1) as SplitButton;

          for (int i = 0; i < 10; i++)
          {
              PushButtonData bOne = new PushButtonData("Button" + i.ToString(), "Семейство " + i.ToString(),
                path, "Hello.HelloOne");
              sb.AddPushButton(bOne);
          }*/

          /*PushButtonData bOne = new PushButtonData("ButtonNameA", "Option One",
           path, "Hello.HelloOne");

          PushButtonData bTwo = new PushButtonData("ButtonNameB", "Option Two",
                  path, "Hello.HelloTwo");

          PushButtonData bThree = new PushButtonData("ButtonNameC", "Option Three",
           path, "Hello.HelloThree");*/


          /*sb.AddPushButton(bOne);
          sb.AddPushButton(bTwo);
          sb.AddPushButton(bThree);*/

          /*RibbonItemData i1 = new PushButtonData(
              "TableLoadPlace", "1 Table Load and Place",
              path, "FamilyApi.CmdTableLoadPlace" );

          i1.ToolTip = "Load the table family and "
            + "place table instances";

          RibbonItemData i2 = new PushButtonData(
            "TableModify", "2 Table New Type Modify",
            path, "FamilyApi.CmdTableNewTypeModify" );

          i2.ToolTip = "Create new table type and "
            + "modify selected instances";*/


          //p.AddStackedItems( i1, i2);
          RibbonItemData i1 = new PushButtonData(
              "TableLoadPlace", "Загрузить семейства",
              path, "FamilyApi.CmdLoadArrayFamily");

          i1.ToolTip = "Load the table family and "
            + "place table instances";
          p.AddItem(i1);
      }

    public Result OnStartup( UIControlledApplication a )
    {
      PopulatePanel( 
        a.CreateRibbonPanel( 
          Util.Caption ) );

      return Result.Succeeded;
    }

    public Result OnShutdown( UIControlledApplication a )
    {
      return Result.Succeeded;
    }
  }
}
