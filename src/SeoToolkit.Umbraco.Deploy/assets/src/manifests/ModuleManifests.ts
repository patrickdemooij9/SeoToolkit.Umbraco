import { SEOTOOLKIT_ROBOTSTXT_ENTITY } from "../Constants";

export const Manifests = [
    {
        name: "SeoToolkit Deploy Queue Entity Action Registrar",
        alias: "SeoToolkit.Deploy.Queue.Registrar",
        type: "deployEntityActionRegistrar",
        actionAlias: "Deploy.EntityAction.Queue",
        forEntityTypes: [
            {
                entityTypes: [SEOTOOLKIT_ROBOTSTXT_ENTITY],
            },
        ],
    },
    {
        name: "SeoToolkit Deploy Partial Restore Entity Action Registrar",
        alias: "SeoToolkit.Deploy.PartialRestore.Registrar",
        type: "deployEntityActionRegistrar",
        actionAlias: "Deploy.EntityAction.PartialRestore",
        forEntityTypes: [
            {
                entityTypes: [SEOTOOLKIT_ROBOTSTXT_ENTITY],
            },
        ],
    },
    {
        name: "SeoToolkit Deploy Partial Restore Entity Action Registrar",
        alias: "SeoToolkit.Deploy.TreeRestore.Registrar",
        type: "deployEntityActionRegistrar",
        actionAlias: "Deploy.EntityAction.TreeRestore",
        forEntityTypes: [
            {
                entityTypes: [SEOTOOLKIT_ROBOTSTXT_ENTITY],
            },
        ],
    },
    {
      type: "deployEntityTypeMapping",
      alias: "SeoToolkit.Deploy.EntityTypeMapping",
      name: "SeoToolkit Deploy Entity Type Mapping",
      entityTypes: {
        [SEOTOOLKIT_ROBOTSTXT_ENTITY]: "seotoolkit-robotstxt"
      },
    },
];
