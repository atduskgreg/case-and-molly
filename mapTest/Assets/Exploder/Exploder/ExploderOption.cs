// Version 1.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using UnityEngine;

/// <summary>
/// exploder local settings
/// assign this class to your "explodable" object for custom exploder settings
/// if this class is not assigned exploder will use global setting
/// </summary>
public class ExploderOption : MonoBehaviour
{
    /// <summary>
    /// mark this object if it is a 2d plane (sprite)
    /// </summary>
    public bool Plane2D = false;

    /// <summary>
    /// NOTE: this works only for vertex color shaders
    /// this color will be assigned to cross-section plane mesh vertex color
    /// </summary>
    public Color CrossSectionVertexColor = Color.white;

    /// <summary>
    /// uv area of fragment material used for cross section
    /// </summary>
    public Vector4 CrossSectionUV = new Vector4(0, 0, 1, 1);

    /// <summary>
    /// extra option for splitting independent parts of a single mesh
    /// </summary>
    public bool SplitMeshIslands = false;

    /// <summary>
    /// duplicate settings to another object
    /// </summary>
    /// <param name="options">another object settings</param>
    public void DuplicateSettings(ExploderOption options)
    {
        options.Plane2D = Plane2D;
        options.CrossSectionVertexColor = CrossSectionVertexColor;
        options.CrossSectionUV = CrossSectionUV;
        options.SplitMeshIslands = SplitMeshIslands;
    }
}
