#r "nuget:FluentAssertions, 6.1.0"
#r "nuget: Moq, 4.16.1"
#r "nuget: Newtonsoft.Json, 12.0.3"
#r "nuget: Remotion.Linq, 2.2.0"
#r "nuget: Stylelabs.M.Scripting.Types, *"
#r "nuget: Stylelabs.M.Sdk, *"

#load "nuget:ScriptUnit, 0.2.0"
#load "CMP - Create public links for linked assets.csx"

using System.Collections.Generic;
using System.Runtime;
using System.Threading.Tasks;

using static ScriptUnit;
using FluentAssertions;
using Moq;

using Stylelabs.M.Base.Querying;
using Stylelabs.M.Scripting.Types.V1_0.Action;
using Stylelabs.M.Sdk;
using Stylelabs.M.Sdk.Clients;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.Contracts.Logging;
using Stylelabs.M.Sdk.Contracts.Querying;
using Stylelabs.M.Sdk.Factories;

Console.Clear();
return await AddTestsFrom<SignInScriptTests>().Execute();

public class SignInScriptTests
{
    private readonly Mock<Stylelabs.M.Sdk.IMClient> _mclientMock;
    private readonly Mock<IActionScriptContext> _contextMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IQueryingClient> _queryingClientMock;
    private readonly Mock<IEntityFactory> _entityFactoryMock;

    public SignInScriptTests(){
        _contextMock = new Mock<IActionScriptContext>();
        _loggerMock = new Mock<ILogger>();
        _mclientMock = new Mock<IMClient>();
        _queryingClientMock = new Mock<IQueryingClient>();
        _entityFactoryMock = new Mock<IEntityFactory>();

        _mclientMock.Setup(x => x.Querying).Returns(_queryingClientMock.Object);
        _mclientMock.Setup(x => x.Logger).Returns(_loggerMock.Object);
        _contextMock.Setup(x => x.TargetId).Returns(1337);
        _mclientMock.Setup(x => x.EntityFactory).Returns(_entityFactoryMock.Object);
    }

    public async void ShouldAddNumbersAsync()
    {
        // arrange
        Exception caughtExcetion = null;

        var queryResult = new FakeIdQueryResult { Items = new List<long> { 1337 } };

         _queryingClientMock.Setup(x => x.QueryIdsAsync(It.IsAny<Query>()))
                .ReturnsAsync(queryResult);

        _entityFactoryMock.Setup(x => x.CreateAsync("M.PublicLink", null))
            .ReturnsAsync(new Mock<IEntity>().Object);

        // act
        try{
            await RunScriptAsync(_mclientMock.Object, _contextMock.Object);
        }
        catch(Exception ex)
        {
            caughtExcetion = ex;
        }
        
        // assert
        caughtExcetion.Should().BeNull();
    }

    internal class FakeIdQueryResult : IIdQueryResult
    {
        public IList<long> Items {get;set;}

        public long TotalNumberOfResults => Items.Count;

        public long Offset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        IList<object> IQueryResult.Items => throw new NotImplementedException();

        public IIdIterator CreateIterator()
        {
            throw new NotImplementedException();
        }
    }
}