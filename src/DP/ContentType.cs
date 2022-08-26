// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
namespace DAZ_Installer.DP {
    /// <summary>
    /// ContentType represents the type of the content for user daz files (DSON user files).
    /// <para>
    /// For example, <c>type: "wearable"</c> found in a DSON user file should have a ContentType of ContentType.Wearable.
    /// </para>
    /// </summary>
    internal enum ContentType
        {
            Scene,
            Scene_Subset,
            Hierachical_Material,
            Preset_Hierarchical_Pose,
            Wearable,
            Character,
            Figure,
            Prop,
            Preset_Properties,
            Preset_Shape,
            Preset_Pose,
            Preset_Material,
            Preset_Shader,
            Preset_Camera,
            Preset_Light,
            Preset_Render_Settings,
            Preset_Simulation_Settings,
            Preset_DFormer,
            Preset_Layered_Image,
            Preset_Puppeteer,
            Modifier, // aka morph
            UV_Set,
            Script,
            Library,
            Program,
            Media,
            Document,
            Geometry,
            DAZ_File,
            Unknown
        }
}