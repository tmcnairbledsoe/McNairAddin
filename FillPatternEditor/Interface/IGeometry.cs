
using System.ComponentModel;
using System.Windows.Media;
using FillPatternEditor.Enums;
using System.Windows;

namespace FillPatternEditor.Interface
{
    /// <summary>
    /// Interface for representing geometry in a drawing context, supporting property change notifications.
    /// </summary>
    public interface IGeometry : INotifyPropertyChanged
    {
        /// <summary>Unique GUID for the geometry instance.</summary>
        Guid GeometryGuid { get; }

        /// <summary>
        /// State of the geometry's placement within the drawing area.
        /// </summary>
        PlacementInAreaState PlacementInAreaState { get; set; }

        /// <summary>
        /// Determines the geometry's position relative to a given rectangle.
        /// </summary>
        PlacementInAreaState GetPlacementState(Rect rectangle);

        /// <summary> Name of the geometry, useful for display or drag-and-drop lists. </summary>
        string Name { get; set; }

        /// <summary>
        /// Indicates if the geometry is fully initialized. A geometry might need multiple points to be fully defined.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary> If true, the geometry is read-only and cannot be edited or selected. </summary>
        bool IsReadOnly { get; set; }

        /// <summary>
        /// Determines the rendering order of this geometry. Higher ZIndex values are drawn on top of lower values. Default is 0.
        /// </summary>
        int ZIndex { get; set; }

        /// <summary> Whether this geometry can be selected by user interaction. </summary>
        bool CanBeSelected { get; set; }

        /// <summary> Whether the geometry is currently selected. </summary>
        bool IsSelected { get; set; }

        /// <summary> Indicates if the mouse is hovering over this geometry. </summary>
        bool IsMouseHover { get; set; }

        /// <summary> Text note attached to this geometry. </summary>
        string Note { get; set; }

        /// <summary>
        /// Gets the bounding box (bottom-left and top-right corners) that encloses the geometry.
        /// </summary>
        void BoundBox(out Point botLeftPnt, out Point topRightPnt);

        /// <summary>
        /// Gets the composite interface for this geometry if it is a composite geometry. (Composite design pattern).
        /// </summary>
        ICompositeGeometry CompositeGeometry { get; }

        /// <summary>
        /// Sets a point for creating or updating the geometry based on user input, until the geometry is fully initialized.
        /// </summary>
        bool SetPoint(Point globalPoint, bool bInit, IDrawingInfo drawingInfo = null, IGeometry geomUnderPnt = null);

        /// <summary>
        /// Draws the geometry in the given drawing context. Uses coordinate systems for global-local transformations.
        /// </summary>
        void Draw(ICoordinateSystem cs, ISelectionDisplayParameters selectionParameters, DrawingContext dc);

        /// <summary>
        /// Gets a dictionary of grip points (points that can be manipulated) for the geometry.
        /// </summary>
        Dictionary<IGeometryGripPoint, Point> GetGripPoints();

        /// <summary>
        /// Sets a specific grip point on the geometry, used for manipulation.
        /// </summary>
        bool SetGripPoint(IGeometryGripPoint gripPntInfo, Point gripGlobalPoint, IDrawingInfo drawingInfo = null, IGeometry geomUnderPnt = null);

        /// <summary>
        /// Returns the nearest point on the geometry to the given global point.
        /// </summary>
        Point GetNearestPoint(Point globalPnt);

        /// <summary>
        /// Moves the geometry's points based on the provided offset.
        /// </summary>
        bool Move(Vector moveOffset, IDrawingInfo drawingInfo, bool bRemoveDependencies);

        /// <summary>
        /// Clones the geometry, used mainly for drag-and-drop operations. Dependencies are not cloned.
        /// </summary>
        IGeometry Clone();

        /// <summary>
        /// Clones the geometry and fills a dictionary mapping old geometries to new ones (useful for restoring dependencies).
        /// </summary>
        IGeometry Clone(ref Dictionary<IGeometry, IGeometry> oldToNewGeometryMap);

        /// <summary>
        /// Gets a list of property names that should be displayed to the user.
        /// </summary>
        List<string> GetDisplayProperties();

        /// <summary>
        /// Gets a list of geometrical property names (like coordinates, radius) for display.
        /// </summary>
        List<string> GetDisplayGeometryProperties();

        /// <summary>
        /// Gets the value of a specific property by its system name.
        /// </summary>
        object GetPropertyValue(string strPropertySystemName);

        /// <summary>
        /// Sets a specific property by its system name and optionally notifies listeners of the change.
        /// </summary>
        bool SetPropertyValue(string strPropertySystemName, object propertyValue, bool bNotifyPropertyChanged, IDrawingInfo drawingInfo, IGeometry relatedGeometry);

        /// <summary>
        /// Returns a relation (dependency) between this geometry and a given point.
        /// </summary>
        IGeometryRelation GetRelation(Point pointOnGeom);

        /// <summary>
        /// Gets a point on this geometry needed by a dependent geometry.
        /// </summary>
        bool GetPointOnGeometry(IGeometryRelation geomRelation, out Point pointOnGeom);

        /// <summary>
        /// Returns a dictionary of dependencies for this geometry, where the key is the grip point index, and the value is a list of geometry relations.
        /// </summary>
        Dictionary<int, List<IGeometryRelation>> Dependencies { get; }

        /// <summary>
        /// Called when a geometry that this geometry depends on changes.
        /// </summary>
        void OnDependencyFired(IGeometry geom);

        /// <summary>
        /// Removes this geometry's dependency on the given geometry.
        /// </summary>
        void RemoveDependency(IGeometry geom);
    }

    public interface IGeometryRelation
    {
        Guid GeometryGuid { get; }

        bool IsInternalCompositeRelation { get; }

        IGeometryRelation Clone(IGeometry newGeom, bool bIsInternalCompositeRelation);
    }

    public interface IDrawingInfo
    {
        bool IsLimited { get; }

        Point BotLeftDrawingPoint { get; }

        Point TopRightDrawingPoint { get; }
    }

    public interface ICompositeGeometry : IGeometry, INotifyPropertyChanged
    {
        List<IGeometry> Children { get; }
    }

    public interface IGeometryGripPoint
    {
        Guid GeometryGuid { get; }

        int GripIndex { get; }
    }

    public interface ISelectionDisplayParameters
    {
        Color SelectedGeometryBorderColor { get; }

        Color SelectedGeometryFillColor { get; }

        double SelectedGeometryBorderThickness { get; }

        Color MouseHoverGeometryBorderColor { get; }

        double MouseHoverGeometryBorderThickness { get; }

        double MouseHoverDistance { get; }
    }

    public interface IGeometryApproximation
    {
        List<Point> GetApproximationPoints(double rDefTolerance = 0.001);
    }

    public interface ICoordinateSystem
    {
        Point GetLocalPoint(Point globalPnt);

        double GetScale();
    }
}
