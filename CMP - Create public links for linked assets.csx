#load "./references/Action.csx"
/**------------ Include above to support intellisense on Content Hub types in editor ----------------**/
// Script Start
using System.Linq;
using System.Threading.Tasks;
using Stylelabs.M.Base.Querying;
using Stylelabs.M.Base.Querying.Linq;
using Stylelabs.M.Framework.Essentials.LoadOptions;
using Stylelabs.M.Scripting.Types.V1_0.Action;
using Stylelabs.M.Sdk;
using Stylelabs.M.Sdk.Contracts.Base;

await RunScriptAsync(MClient, Context);

public static async Task RunScriptAsync(IMClient mClient, IActionScriptContext context)
{
    if(mClient == null || context == null)
    {
        return;
    }
    
    var assetId = context.TargetId;

    // Check if public links don't exist yet
    var query = Query.CreateQuery(entities => from e in entities
                                                where e.DefinitionName == "M.PublicLink"
                                                && e.Parent("AssetToPublicLink") == assetId.Value
                                                && (e.Property("Resource") == "preview" || e.Property("Resource") == "downloadOriginal")
                                                && e.Property("IsDisabled") == false
                                                select e);
    query.Take = 1;

     var result = await mClient.Querying.QueryIdsAsync(query);
    if (result.TotalNumberOfResults > 0)
    {
        mClient.Logger.Info("Public links already exist for asset with id '" + assetId + "'");
        return;
    }

    // // Create public links
    await CreateForRendition("preview", assetId.Value);
    mClient.Logger.Info("Created public link 'preview' for asset with id '" + assetId + "'");

    await CreateForRendition("downloadOriginal", assetId.Value);
    mClient.Logger.Info("Created public link 'downloadOriginal' for asset with id '" + assetId + "'");

    async Task CreateForRendition(string rendition, long assetId)
    {
        var publicLink = await mClient.EntityFactory.CreateAsync("M.PublicLink");

        if (publicLink.CanDoLazyLoading())
        {
            await publicLink.LoadMembersAsync(new PropertyLoadOption("Resource"), new RelationLoadOption("AssetToPublicLink"));
        }

        publicLink.SetPropertyValue("Resource", rendition);

        var relation = publicLink.GetRelation<IChildToManyParentsRelation>("AssetToPublicLink");
        if (relation == null)
        {
            mClient.Logger.Error("Unable to create public link: no AssetToPublicLink relation found.");
            return;
        }

        relation.Parents.Add(assetId);

        await mClient.Entities.SaveAsync(publicLink);
        return;
    }
}