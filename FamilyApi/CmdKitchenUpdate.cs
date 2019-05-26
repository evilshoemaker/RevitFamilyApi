#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

namespace FamilyApi
{
  /// <summary>
  /// Select all kitchen cabinets, display all
  /// applicable door panel types, and apply the 
  /// selected type to all cabinets.
  /// </summary>
  [Transaction( TransactionMode.Manual )]
  public class CmdKitchenUpdate : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Application app = uiapp.Application;
      Document doc = uidoc.Document;

      // Retrieve all door panel types

      List<Element> door_panel_types = new List<Element>(
        new FilteredElementCollector( doc )
          .OfCategory( BuiltInCategory.OST_GenericModel )
          .OfClass( typeof( FamilySymbol ) )
          .Where<Element>( e => e.Name.StartsWith( 
            "Door Panel - " ) ) );

      // Retrieve all cabinet instances 
      // with a door panel type parameter:

      IEnumerable<Element> casework
        = new FilteredElementCollector( doc )
          .OfCategory( BuiltInCategory.OST_Casework )
          .OfClass( typeof( FamilyInstance ) )
          //.Cast<FamilyInstance>()
          .Where<Element>( e => 
            (null != e.get_Parameter( 
              "Door Panel Type" )) );

      // Determine currently selected door panel type

      string current_door_panel_type_name = null;

      foreach( Element e in casework )
      {
        Debug.Print( Util.ElementDescription( e ) );

        Parameter p = e.get_Parameter( 
          "Door Panel Type" );

        string name = doc.GetElement( p.AsElementId() )
          .Name;

        if( null == current_door_panel_type_name )
        {
          current_door_panel_type_name = name;
        }
        else if( !current_door_panel_type_name.Equals(
          name ) )
        {
          current_door_panel_type_name = "*VARIES*";
          break;
        }
      }

      // Display form to select new door panel type

      DoorPanelTypeSelectorForm form
        = new DoorPanelTypeSelectorForm( 
          current_door_panel_type_name, 
          door_panel_types );

      if( System.Windows.Forms.DialogResult.OK
        == form.ShowDialog() )
      {
        FamilySymbol door_panel_type
          = form.SelectedItem as FamilySymbol;

        ElementId id = door_panel_type.Id;

        using( Transaction tx = new Transaction( doc ) )
        {
          tx.Start( "Modify Door Panel Type" );

          foreach( Element e in casework )
          {
            Parameter p = e.get_Parameter( 
              "Door Panel Type" );

            p.Set( id );
          }
          tx.Commit();
        }
      }
      return Result.Succeeded;
    }
  }
}
