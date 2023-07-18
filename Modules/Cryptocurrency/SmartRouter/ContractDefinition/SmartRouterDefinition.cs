using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace PancakeRouter.SmartRouter.ContractDefinition;

public class SmartRouterDeployment : SmartRouterDeploymentBase
{
    public SmartRouterDeployment() : base(BYTECODE)
    {
    }

    public SmartRouterDeployment(string byteCode) : base(byteCode)
    {
    }
}

public class SmartRouterDeploymentBase : ContractDeploymentMessage
{
    public static string BYTECODE = "0x";

    public SmartRouterDeploymentBase() : base(BYTECODE)
    {
    }

    public SmartRouterDeploymentBase(string byteCode) : base(byteCode)
    {
    }

    [Parameter("address", "_factory")] public virtual string Factory { get; set; }

    [Parameter("address", "_WETH", 2)] public virtual string Weth { get; set; }
}

public class WethFunction : WethFunctionBase
{
}

[Function("WETH", "address")]
public class WethFunctionBase : FunctionMessage
{
}

public class AddLiquidityFunction : AddLiquidityFunctionBase
{
}

[Function("addLiquidity", typeof(AddLiquidityOutputDTO))]
public class AddLiquidityFunctionBase : FunctionMessage
{
    [Parameter("address", "tokenA")] public virtual string TokenA { get; set; }

    [Parameter("address", "tokenB", 2)] public virtual string TokenB { get; set; }

    [Parameter("uint256", "amountADesired", 3)]
    public virtual BigInteger AmountADesired { get; set; }

    [Parameter("uint256", "amountBDesired", 4)]
    public virtual BigInteger AmountBDesired { get; set; }

    [Parameter("uint256", "amountAMin", 5)]
    public virtual BigInteger AmountAMin { get; set; }

    [Parameter("uint256", "amountBMin", 6)]
    public virtual BigInteger AmountBMin { get; set; }

    [Parameter("address", "to", 7)] public virtual string To { get; set; }

    [Parameter("uint256", "deadline", 8)] public virtual BigInteger Deadline { get; set; }
}

public class AddLiquidityETHFunction : AddLiquidityETHFunctionBase
{
}

[Function("addLiquidityETH", typeof(AddLiquidityETHOutputDTO))]
public class AddLiquidityETHFunctionBase : FunctionMessage
{
    [Parameter("address", "token")] public virtual string Token { get; set; }

    [Parameter("uint256", "amountTokenDesired", 2)]
    public virtual BigInteger AmountTokenDesired { get; set; }

    [Parameter("uint256", "amountTokenMin", 3)]
    public virtual BigInteger AmountTokenMin { get; set; }

    [Parameter("uint256", "amountETHMin", 4)]
    public virtual BigInteger AmountETHMin { get; set; }

    [Parameter("address", "to", 5)] public virtual string To { get; set; }

    [Parameter("uint256", "deadline", 6)] public virtual BigInteger Deadline { get; set; }
}

public class FactoryFunction : FactoryFunctionBase
{
}

[Function("factory", "address")]
public class FactoryFunctionBase : FunctionMessage
{
}

public class GetAmountInFunction : GetAmountInFunctionBase
{
}

[Function("getAmountIn", "uint256")]
public class GetAmountInFunctionBase : FunctionMessage
{
    [Parameter("uint256", "amountOut")] public virtual BigInteger AmountOut { get; set; }

    [Parameter("uint256", "reserveIn", 2)] public virtual BigInteger ReserveIn { get; set; }

    [Parameter("uint256", "reserveOut", 3)]
    public virtual BigInteger ReserveOut { get; set; }
}

public class GetAmountOutFunction : GetAmountOutFunctionBase
{
}

[Function("getAmountOut", "uint256")]
public class GetAmountOutFunctionBase : FunctionMessage
{
    [Parameter("uint256", "amountIn")] public virtual BigInteger AmountIn { get; set; }

    [Parameter("uint256", "reserveIn", 2)] public virtual BigInteger ReserveIn { get; set; }

    [Parameter("uint256", "reserveOut", 3)]
    public virtual BigInteger ReserveOut { get; set; }
}

public class GetAmountsInFunction : GetAmountsInFunctionBase
{
}

[Function("getAmountsIn", "uint256[]")]
public class GetAmountsInFunctionBase : FunctionMessage
{
    [Parameter("uint256", "amountOut")] public virtual BigInteger AmountOut { get; set; }

    [Parameter("address[]", "path", 2)] public virtual List<string> Path { get; set; }
}

public class GetAmountsOutFunction : GetAmountsOutFunctionBase
{
}

[Function("getAmountsOut", "uint256[]")]
public class GetAmountsOutFunctionBase : FunctionMessage
{
    [Parameter("uint256", "amountIn")] public virtual BigInteger AmountIn { get; set; }

    [Parameter("address[]", "path", 2)] public virtual List<string> Path { get; set; }
}

public class QuoteFunction : QuoteFunctionBase
{
}

[Function("quote", "uint256")]
public class QuoteFunctionBase : FunctionMessage
{
    [Parameter("uint256", "amountA")] public virtual BigInteger AmountA { get; set; }

    [Parameter("uint256", "reserveA", 2)] public virtual BigInteger ReserveA { get; set; }

    [Parameter("uint256", "reserveB", 3)] public virtual BigInteger ReserveB { get; set; }
}

public class RemoveLiquidityFunction : RemoveLiquidityFunctionBase
{
}

[Function("removeLiquidity", typeof(RemoveLiquidityOutputDTO))]
public class RemoveLiquidityFunctionBase : FunctionMessage
{
    [Parameter("address", "tokenA")] public virtual string TokenA { get; set; }

    [Parameter("address", "tokenB", 2)] public virtual string TokenB { get; set; }

    [Parameter("uint256", "liquidity", 3)] public virtual BigInteger Liquidity { get; set; }

    [Parameter("uint256", "amountAMin", 4)]
    public virtual BigInteger AmountAMin { get; set; }

    [Parameter("uint256", "amountBMin", 5)]
    public virtual BigInteger AmountBMin { get; set; }

    [Parameter("address", "to", 6)] public virtual string To { get; set; }

    [Parameter("uint256", "deadline", 7)] public virtual BigInteger Deadline { get; set; }
}

public class RemoveLiquidityETHFunction : RemoveLiquidityETHFunctionBase
{
}

[Function("removeLiquidityETH", typeof(RemoveLiquidityETHOutputDTO))]
public class RemoveLiquidityETHFunctionBase : FunctionMessage
{
    [Parameter("address", "token")] public virtual string Token { get; set; }

    [Parameter("uint256", "liquidity", 2)] public virtual BigInteger Liquidity { get; set; }

    [Parameter("uint256", "amountTokenMin", 3)]
    public virtual BigInteger AmountTokenMin { get; set; }

    [Parameter("uint256", "amountETHMin", 4)]
    public virtual BigInteger AmountETHMin { get; set; }

    [Parameter("address", "to", 5)] public virtual string To { get; set; }

    [Parameter("uint256", "deadline", 6)] public virtual BigInteger Deadline { get; set; }
}

public class
    RemoveLiquidityETHSupportingFeeOnTransferTokensFunction :
        RemoveLiquidityETHSupportingFeeOnTransferTokensFunctionBase
{
}

[Function("removeLiquidityETHSupportingFeeOnTransferTokens", "uint256")]
public class RemoveLiquidityETHSupportingFeeOnTransferTokensFunctionBase : FunctionMessage
{
    [Parameter("address", "token")] public virtual string Token { get; set; }

    [Parameter("uint256", "liquidity", 2)] public virtual BigInteger Liquidity { get; set; }

    [Parameter("uint256", "amountTokenMin", 3)]
    public virtual BigInteger AmountTokenMin { get; set; }

    [Parameter("uint256", "amountETHMin", 4)]
    public virtual BigInteger AmountETHMin { get; set; }

    [Parameter("address", "to", 5)] public virtual string To { get; set; }

    [Parameter("uint256", "deadline", 6)] public virtual BigInteger Deadline { get; set; }
}

public class RemoveLiquidityETHWithPermitFunction : RemoveLiquidityETHWithPermitFunctionBase
{
}

[Function("removeLiquidityETHWithPermit", typeof(RemoveLiquidityETHWithPermitOutputDTO))]
public class RemoveLiquidityETHWithPermitFunctionBase : FunctionMessage
{
    [Parameter("address", "token")] public virtual string Token { get; set; }

    [Parameter("uint256", "liquidity", 2)] public virtual BigInteger Liquidity { get; set; }

    [Parameter("uint256", "amountTokenMin", 3)]
    public virtual BigInteger AmountTokenMin { get; set; }

    [Parameter("uint256", "amountETHMin", 4)]
    public virtual BigInteger AmountETHMin { get; set; }

    [Parameter("address", "to", 5)] public virtual string To { get; set; }

    [Parameter("uint256", "deadline", 6)] public virtual BigInteger Deadline { get; set; }

    [Parameter("bool", "approveMax", 7)] public virtual bool ApproveMax { get; set; }

    [Parameter("uint8", "v", 8)] public virtual byte V { get; set; }

    [Parameter("bytes32", "r", 9)] public virtual byte[] R { get; set; }

    [Parameter("bytes32", "s", 10)] public virtual byte[] S { get; set; }
}

public class
    RemoveLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction :
        RemoveLiquidityETHWithPermitSupportingFeeOnTransferTokensFunctionBase
{
}

[Function("removeLiquidityETHWithPermitSupportingFeeOnTransferTokens", "uint256")]
public class RemoveLiquidityETHWithPermitSupportingFeeOnTransferTokensFunctionBase : FunctionMessage
{
    [Parameter("address", "token")] public virtual string Token { get; set; }

    [Parameter("uint256", "liquidity", 2)] public virtual BigInteger Liquidity { get; set; }

    [Parameter("uint256", "amountTokenMin", 3)]
    public virtual BigInteger AmountTokenMin { get; set; }

    [Parameter("uint256", "amountETHMin", 4)]
    public virtual BigInteger AmountETHMin { get; set; }

    [Parameter("address", "to", 5)] public virtual string To { get; set; }

    [Parameter("uint256", "deadline", 6)] public virtual BigInteger Deadline { get; set; }

    [Parameter("bool", "approveMax", 7)] public virtual bool ApproveMax { get; set; }

    [Parameter("uint8", "v", 8)] public virtual byte V { get; set; }

    [Parameter("bytes32", "r", 9)] public virtual byte[] R { get; set; }

    [Parameter("bytes32", "s", 10)] public virtual byte[] S { get; set; }
}

public class RemoveLiquidityWithPermitFunction : RemoveLiquidityWithPermitFunctionBase
{
}

[Function("removeLiquidityWithPermit", typeof(RemoveLiquidityWithPermitOutputDTO))]
public class RemoveLiquidityWithPermitFunctionBase : FunctionMessage
{
    [Parameter("address", "tokenA")] public virtual string TokenA { get; set; }

    [Parameter("address", "tokenB", 2)] public virtual string TokenB { get; set; }

    [Parameter("uint256", "liquidity", 3)] public virtual BigInteger Liquidity { get; set; }

    [Parameter("uint256", "amountAMin", 4)]
    public virtual BigInteger AmountAMin { get; set; }

    [Parameter("uint256", "amountBMin", 5)]
    public virtual BigInteger AmountBMin { get; set; }

    [Parameter("address", "to", 6)] public virtual string To { get; set; }

    [Parameter("uint256", "deadline", 7)] public virtual BigInteger Deadline { get; set; }

    [Parameter("bool", "approveMax", 8)] public virtual bool ApproveMax { get; set; }

    [Parameter("uint8", "v", 9)] public virtual byte V { get; set; }

    [Parameter("bytes32", "r", 10)] public virtual byte[] R { get; set; }

    [Parameter("bytes32", "s", 11)] public virtual byte[] S { get; set; }
}

public class SwapETHForExactTokensFunction : SwapETHForExactTokensFunctionBase
{
}

[Function("swapETHForExactTokens", "uint256[]")]
public class SwapETHForExactTokensFunctionBase : FunctionMessage
{
    [Parameter("uint256", "amountOut")] public virtual BigInteger AmountOut { get; set; }

    [Parameter("address[]", "path", 2)] public virtual List<string> Path { get; set; }

    [Parameter("address", "to", 3)] public virtual string To { get; set; }

    [Parameter("uint256", "deadline", 4)] public virtual BigInteger Deadline { get; set; }
}

public class SwapExactETHForTokensFunction : SwapExactETHForTokensFunctionBase
{
}

[Function("swapExactETHForTokens", "uint256[]")]
public class SwapExactETHForTokensFunctionBase : FunctionMessage
{
    [Parameter("uint256", "amountOutMin")] public virtual BigInteger AmountOutMin { get; set; }

    [Parameter("address[]", "path", 2)] public virtual List<string> Path { get; set; }

    [Parameter("address", "to", 3)] public virtual string To { get; set; }

    [Parameter("uint256", "deadline", 4)] public virtual BigInteger Deadline { get; set; }
}

public class
    SwapExactETHForTokensSupportingFeeOnTransferTokensFunction :
        SwapExactETHForTokensSupportingFeeOnTransferTokensFunctionBase
{
}

[Function("swapExactETHForTokensSupportingFeeOnTransferTokens")]
public class SwapExactETHForTokensSupportingFeeOnTransferTokensFunctionBase : FunctionMessage
{
    [Parameter("uint256", "amountOutMin")] public virtual BigInteger AmountOutMin { get; set; }

    [Parameter("address[]", "path", 2)] public virtual List<string> Path { get; set; }

    [Parameter("address", "to", 3)] public virtual string To { get; set; }

    [Parameter("uint256", "deadline", 4)] public virtual BigInteger Deadline { get; set; }
}

public class SwapExactTokensForETHFunction : SwapExactTokensForETHFunctionBase
{
}

[Function("swapExactTokensForETH", "uint256[]")]
public class SwapExactTokensForETHFunctionBase : FunctionMessage
{
    [Parameter("uint256", "amountIn")] public virtual BigInteger AmountIn { get; set; }

    [Parameter("uint256", "amountOutMin", 2)]
    public virtual BigInteger AmountOutMin { get; set; }

    [Parameter("address[]", "path", 3)] public virtual List<string> Path { get; set; }

    [Parameter("address", "to", 4)] public virtual string To { get; set; }

    [Parameter("uint256", "deadline", 5)] public virtual BigInteger Deadline { get; set; }
}

public class
    SwapExactTokensForETHSupportingFeeOnTransferTokensFunction :
        SwapExactTokensForETHSupportingFeeOnTransferTokensFunctionBase
{
}

[Function("swapExactTokensForETHSupportingFeeOnTransferTokens")]
public class SwapExactTokensForETHSupportingFeeOnTransferTokensFunctionBase : FunctionMessage
{
    [Parameter("uint256", "amountIn")] public virtual BigInteger AmountIn { get; set; }

    [Parameter("uint256", "amountOutMin", 2)]
    public virtual BigInteger AmountOutMin { get; set; }

    [Parameter("address[]", "path", 3)] public virtual List<string> Path { get; set; }

    [Parameter("address", "to", 4)] public virtual string To { get; set; }

    [Parameter("uint256", "deadline", 5)] public virtual BigInteger Deadline { get; set; }
}

public class SwapExactTokensForTokensFunction : SwapExactTokensForTokensFunctionBase
{
}

[Function("swapExactTokensForTokens", "uint256[]")]
public class SwapExactTokensForTokensFunctionBase : FunctionMessage
{
    [Parameter("uint256", "amountIn")] public virtual BigInteger AmountIn { get; set; }

    [Parameter("uint256", "amountOutMin", 2)]
    public virtual BigInteger AmountOutMin { get; set; }

    [Parameter("address[]", "path", 3)] public virtual List<string> Path { get; set; }

    [Parameter("address", "to", 4)] public virtual string To { get; set; }

    [Parameter("uint256", "deadline", 5)] public virtual BigInteger Deadline { get; set; }
}

public class
    SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction :
        SwapExactTokensForTokensSupportingFeeOnTransferTokensFunctionBase
{
}

[Function("swapExactTokensForTokensSupportingFeeOnTransferTokens")]
public class SwapExactTokensForTokensSupportingFeeOnTransferTokensFunctionBase : FunctionMessage
{
    [Parameter("uint256", "amountIn")] public virtual BigInteger AmountIn { get; set; }

    [Parameter("uint256", "amountOutMin", 2)]
    public virtual BigInteger AmountOutMin { get; set; }

    [Parameter("address[]", "path", 3)] public virtual List<string> Path { get; set; }

    [Parameter("address", "to", 4)] public virtual string To { get; set; }

    [Parameter("uint256", "deadline", 5)] public virtual BigInteger Deadline { get; set; }
}

public class SwapTokensForExactETHFunction : SwapTokensForExactETHFunctionBase
{
}

[Function("swapTokensForExactETH", "uint256[]")]
public class SwapTokensForExactETHFunctionBase : FunctionMessage
{
    [Parameter("uint256", "amountOut")] public virtual BigInteger AmountOut { get; set; }

    [Parameter("uint256", "amountInMax", 2)]
    public virtual BigInteger AmountInMax { get; set; }

    [Parameter("address[]", "path", 3)] public virtual List<string> Path { get; set; }

    [Parameter("address", "to", 4)] public virtual string To { get; set; }

    [Parameter("uint256", "deadline", 5)] public virtual BigInteger Deadline { get; set; }
}

public class SwapTokensForExactTokensFunction : SwapTokensForExactTokensFunctionBase
{
}

[Function("swapTokensForExactTokens", "uint256[]")]
public class SwapTokensForExactTokensFunctionBase : FunctionMessage
{
    [Parameter("uint256", "amountOut")] public virtual BigInteger AmountOut { get; set; }

    [Parameter("uint256", "amountInMax", 2)]
    public virtual BigInteger AmountInMax { get; set; }

    [Parameter("address[]", "path", 3)] public virtual List<string> Path { get; set; }

    [Parameter("address", "to", 4)] public virtual string To { get; set; }

    [Parameter("uint256", "deadline", 5)] public virtual BigInteger Deadline { get; set; }
}

public class WethOutputDTO : WethOutputDTOBase
{
}

[FunctionOutput]
public class WethOutputDTOBase : IFunctionOutputDTO
{
    [Parameter("address", "")] public virtual string ReturnValue1 { get; set; }
}

public class AddLiquidityOutputDTO : AddLiquidityOutputDTOBase
{
}

[FunctionOutput]
public class AddLiquidityOutputDTOBase : IFunctionOutputDTO
{
    [Parameter("uint256", "amountA")] public virtual BigInteger AmountA { get; set; }

    [Parameter("uint256", "amountB", 2)] public virtual BigInteger AmountB { get; set; }

    [Parameter("uint256", "liquidity", 3)] public virtual BigInteger Liquidity { get; set; }
}

public class AddLiquidityETHOutputDTO : AddLiquidityETHOutputDTOBase
{
}

[FunctionOutput]
public class AddLiquidityETHOutputDTOBase : IFunctionOutputDTO
{
    [Parameter("uint256", "amountToken")] public virtual BigInteger AmountToken { get; set; }

    [Parameter("uint256", "amountETH", 2)] public virtual BigInteger AmountETH { get; set; }

    [Parameter("uint256", "liquidity", 3)] public virtual BigInteger Liquidity { get; set; }
}

public class FactoryOutputDTO : FactoryOutputDTOBase
{
}

[FunctionOutput]
public class FactoryOutputDTOBase : IFunctionOutputDTO
{
    [Parameter("address", "")] public virtual string ReturnValue1 { get; set; }
}

public class GetAmountInOutputDTO : GetAmountInOutputDTOBase
{
}

[FunctionOutput]
public class GetAmountInOutputDTOBase : IFunctionOutputDTO
{
    [Parameter("uint256", "amountIn")] public virtual BigInteger AmountIn { get; set; }
}

public class GetAmountOutOutputDTO : GetAmountOutOutputDTOBase
{
}

[FunctionOutput]
public class GetAmountOutOutputDTOBase : IFunctionOutputDTO
{
    [Parameter("uint256", "amountOut")] public virtual BigInteger AmountOut { get; set; }
}

public class GetAmountsInOutputDTO : GetAmountsInOutputDTOBase
{
}

[FunctionOutput]
public class GetAmountsInOutputDTOBase : IFunctionOutputDTO
{
    [Parameter("uint256[]", "amounts")] public virtual List<BigInteger> Amounts { get; set; }
}

public class GetAmountsOutOutputDTO : GetAmountsOutOutputDTOBase
{
}

[FunctionOutput]
public class GetAmountsOutOutputDTOBase : IFunctionOutputDTO
{
    [Parameter("uint256[]", "amounts")] public virtual List<BigInteger> Amounts { get; set; }
}

public class QuoteOutputDTO : QuoteOutputDTOBase
{
}

[FunctionOutput]
public class QuoteOutputDTOBase : IFunctionOutputDTO
{
    [Parameter("uint256", "amountB")] public virtual BigInteger AmountB { get; set; }
}

public class RemoveLiquidityOutputDTO : RemoveLiquidityOutputDTOBase
{
}

[FunctionOutput]
public class RemoveLiquidityOutputDTOBase : IFunctionOutputDTO
{
    [Parameter("uint256", "amountA")] public virtual BigInteger AmountA { get; set; }

    [Parameter("uint256", "amountB", 2)] public virtual BigInteger AmountB { get; set; }
}

public class RemoveLiquidityETHOutputDTO : RemoveLiquidityETHOutputDTOBase
{
}

[FunctionOutput]
public class RemoveLiquidityETHOutputDTOBase : IFunctionOutputDTO
{
    [Parameter("uint256", "amountToken")] public virtual BigInteger AmountToken { get; set; }

    [Parameter("uint256", "amountETH", 2)] public virtual BigInteger AmountETH { get; set; }
}

public class RemoveLiquidityETHWithPermitOutputDTO : RemoveLiquidityETHWithPermitOutputDTOBase
{
}

[FunctionOutput]
public class RemoveLiquidityETHWithPermitOutputDTOBase : IFunctionOutputDTO
{
    [Parameter("uint256", "amountToken")] public virtual BigInteger AmountToken { get; set; }

    [Parameter("uint256", "amountETH", 2)] public virtual BigInteger AmountETH { get; set; }
}

public class RemoveLiquidityWithPermitOutputDTO : RemoveLiquidityWithPermitOutputDTOBase
{
}

[FunctionOutput]
public class RemoveLiquidityWithPermitOutputDTOBase : IFunctionOutputDTO
{
    [Parameter("uint256", "amountA")] public virtual BigInteger AmountA { get; set; }

    [Parameter("uint256", "amountB", 2)] public virtual BigInteger AmountB { get; set; }
}