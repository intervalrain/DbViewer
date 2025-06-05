using Common.Modularity;
using Common.Modularity.Attributes;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Common.Test.Modularity;

public class ModuleRegisterTests
{
    [Fact]
    public void ConfigureModules_StartWithRoot_ShouldConfigureAllModulesSuccessfully()
    {
        // Arrange
        string[][] expected = [["H","F","G","E","C","D","B","A"],
                               ["H","F","G","C","E","D","B","A"],
                               ["F","H","G","E","C","D","B","A"],
                               ["F","H","G","C","E","D","B","A"]];
        var services = new ServiceCollection();

        // Act
        var context = ModuleRegister.ConfigureModules(services, typeof(AModule));

        // Assert
        var order = context.Configures.Select((config, index) => new { name = config.Name, index })
            .ToDictionary(x => x.name, x => x.index);

        order.Count.ShouldBe(expected[0].Length);

        // A 依賴 B,C -> A 應該在 B,C 之後
        order["A"].ShouldBeGreaterThan(order["B"], "A should come after B");
        order["A"].ShouldBeGreaterThan(order["C"], "A should come after C");

        // B 依賴 C,D,F -> B 應該在 C,D,F 之後
        order["B"].ShouldBeGreaterThan(order["C"], "B should come after C");
        order["B"].ShouldBeGreaterThan(order["D"], "B should come after D");
        order["B"].ShouldBeGreaterThan(order["F"], "B should come after F");

        // C 依賴 G,H -> C 應該在 G,H 之後
        order["C"].ShouldBeGreaterThan(order["G"], "C should come after G");
        order["C"].ShouldBeGreaterThan(order["H"], "C should come after H");

        // D 依賴 E -> D 應該在 E 之後
        order["D"].ShouldBeGreaterThan(order["E"], "D should come after E");

        // E 依賴 F,G,H -> E 應該在 F,G,H 之後
        order["E"].ShouldBeGreaterThan(order["F"], "E should come after F");
        order["E"].ShouldBeGreaterThan(order["G"], "E should come after G");
        order["E"].ShouldBeGreaterThan(order["H"], "E should come after H");

        // G 依賴 H -> G 應該在 H 之後
        order["G"].ShouldBeGreaterThan(order["H"], "G should come after H");
    }

    [Fact]
    public void ConfigureModules_StartNotWithRoot_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            var context = ModuleRegister.ConfigureModules(services, typeof(DModule));
        });
    }
}

// A->{B,C}  
// B->{C,D,F}
// C->{G,H}  
// D->{E}    
// E->{F,G,H}
// F->{}     
// G->{H}    
// H->{}     
//
// A,B,D,{C,E},G,{F,H}

public abstract class ModuleBase : Module
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton(GetType());
    }
}

[DependsOn(typeof(BModule), typeof(CModule))] public class AModule : ModuleBase { }
[DependsOn(typeof(CModule), typeof(DModule), typeof(FModule))] public class BModule : ModuleBase { }
[DependsOn(typeof(GModule), typeof(HModule))] public class CModule : ModuleBase { }
[DependsOn(typeof(EModule))] public class DModule : ModuleBase { }
[DependsOn(typeof(FModule), typeof(GModule), typeof(HModule))] public class EModule : ModuleBase { }
[DependsOn] public class FModule : ModuleBase { }
[DependsOn(typeof(HModule))] public class GModule : ModuleBase { }
[DependsOn] public class HModule : ModuleBase { }
