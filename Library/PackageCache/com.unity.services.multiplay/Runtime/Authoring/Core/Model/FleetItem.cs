using System;
using Unity.Services.DeploymentApi.Editor;
using Unity.Services.Multiplay.Authoring.Core.Assets;

namespace Unity.Services.Multiplay.Authoring.Core.Model
{
    [Serializable]
    class FleetItem : DeploymentItem, IResourceRef
    {
        public FleetItem()
        {
            Type = "Fleet";
        }

        public FleetName OriginalName { get; set; }

        public MultiplayConfig.FleetDefinition Definition { get; set; }
        public IResourceName ResourceName => OriginalName;
    }
}
