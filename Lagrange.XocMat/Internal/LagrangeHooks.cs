using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Lagrange.XocMat.Internal;

public class LagrangeHooks(ILogger<LagrangeHooks> logger) : IHostedService
{
    private readonly ILogger<LagrangeHooks> _logger = logger;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        IL.Lagrange.Core.Message.Entity.ForwardEntity.PackElement += ForwardEntity_PackElement;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 干掉ForwardEntity的@成员
    /// </summary>
    /// <param name="il"></param>
    private void ForwardEntity_PackElement(MonoMod.Cil.ILContext il)
    {
        var instr = il.Instrs.Last(i => i.OpCode == OpCodes.Callvirt && i.Operand is MethodReference rm && rm.Name == "get_ClientSequence");
        var sourceTarget = instr.Next.Operand;
        var i = il.IndexOf(instr);
        il.Instrs.Insert(i + 1, Instruction.Create(OpCodes.Ldc_I4_5));
        il.Instrs.Insert(i + 2, Instruction.Create(OpCodes.Ceq));
        il.Instrs[i + 3].OpCode = OpCodes.Brfalse_S;
        _logger.LogInformation("Modification ForwardEntity Il prohibit @ member");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        IL.Lagrange.Core.Message.Entity.ForwardEntity.PackElement -= ForwardEntity_PackElement;
        return Task.CompletedTask;
    }
}