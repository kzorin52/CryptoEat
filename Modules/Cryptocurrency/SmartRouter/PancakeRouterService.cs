using System.Numerics;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using PancakeRouter.SmartRouter.ContractDefinition;

namespace PancakeRouter.SmartRouter;

public class PancakeRouterService
{
    public PancakeRouterService(Web3 web3, string contractAddress)
    {
        Web3 = web3;
        ContractHandler = web3.Eth.GetContractHandler(contractAddress);
    }

    public PancakeRouterService(IWeb3 web3, string contractAddress)
    {
        Web3 = web3;
        ContractHandler = web3.Eth.GetContractHandler(contractAddress);
    }

    protected IWeb3 Web3 { get; }

    public ContractHandler ContractHandler { get; }

    public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Web3 web3,
        SmartRouterDeployment smartRouterDeployment, CancellationTokenSource cancellationTokenSource = null)
    {
        return web3.Eth.GetContractDeploymentHandler<SmartRouterDeployment>()
            .SendRequestAndWaitForReceiptAsync(smartRouterDeployment, cancellationTokenSource);
    }

    public static Task<string> DeployContractAsync(Web3 web3, SmartRouterDeployment smartRouterDeployment)
    {
        return web3.Eth.GetContractDeploymentHandler<SmartRouterDeployment>()
            .SendRequestAsync(smartRouterDeployment);
    }

    public static async Task<PancakeRouterService> DeployContractAndGetServiceAsync(Web3 web3,
        SmartRouterDeployment smartRouterDeployment, CancellationTokenSource cancellationTokenSource = null)
    {
        var receipt =
            await DeployContractAndWaitForReceiptAsync(web3, smartRouterDeployment, cancellationTokenSource);
        return new PancakeRouterService(web3, receipt.ContractAddress);
    }

    public Task<string> WethQueryAsync(WethFunction wethFunction, BlockParameter blockParameter = null)
    {
        return ContractHandler.QueryAsync<WethFunction, string>(wethFunction, blockParameter);
    }


    public Task<string> WethQueryAsync(BlockParameter blockParameter = null)
    {
        return ContractHandler.QueryAsync<WethFunction, string>(null, blockParameter);
    }

    public Task<string> AddLiquidityRequestAsync(AddLiquidityFunction addLiquidityFunction)
    {
        return ContractHandler.SendRequestAsync(addLiquidityFunction);
    }

    public Task<TransactionReceipt> AddLiquidityRequestAndWaitForReceiptAsync(
        AddLiquidityFunction addLiquidityFunction, CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(addLiquidityFunction, cancellationToken);
    }

    public Task<string> AddLiquidityRequestAsync(string tokenA, string tokenB, BigInteger amountADesired,
        BigInteger amountBDesired, BigInteger amountAMin, BigInteger amountBMin, string to, BigInteger deadline)
    {
        var addLiquidityFunction = new AddLiquidityFunction
        {
            TokenA = tokenA,
            TokenB = tokenB,
            AmountADesired = amountADesired,
            AmountBDesired = amountBDesired,
            AmountAMin = amountAMin,
            AmountBMin = amountBMin,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAsync(addLiquidityFunction);
    }

    public Task<TransactionReceipt> AddLiquidityRequestAndWaitForReceiptAsync(string tokenA, string tokenB,
        BigInteger amountADesired, BigInteger amountBDesired, BigInteger amountAMin, BigInteger amountBMin,
        string to, BigInteger deadline, CancellationTokenSource cancellationToken = null)
    {
        var addLiquidityFunction = new AddLiquidityFunction
        {
            TokenA = tokenA,
            TokenB = tokenB,
            AmountADesired = amountADesired,
            AmountBDesired = amountBDesired,
            AmountAMin = amountAMin,
            AmountBMin = amountBMin,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(addLiquidityFunction, cancellationToken);
    }

    public Task<string> AddLiquidityETHRequestAsync(AddLiquidityETHFunction addLiquidityETHFunction)
    {
        return ContractHandler.SendRequestAsync(addLiquidityETHFunction);
    }

    public Task<TransactionReceipt> AddLiquidityETHRequestAndWaitForReceiptAsync(
        AddLiquidityETHFunction addLiquidityETHFunction, CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(addLiquidityETHFunction, cancellationToken);
    }

    public Task<string> AddLiquidityETHRequestAsync(string token, BigInteger amountTokenDesired,
        BigInteger amountTokenMin, BigInteger amountETHMin, string to, BigInteger deadline)
    {
        var addLiquidityETHFunction = new AddLiquidityETHFunction
        {
            Token = token,
            AmountTokenDesired = amountTokenDesired,
            AmountTokenMin = amountTokenMin,
            AmountETHMin = amountETHMin,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAsync(addLiquidityETHFunction);
    }

    public Task<TransactionReceipt> AddLiquidityETHRequestAndWaitForReceiptAsync(string token,
        BigInteger amountTokenDesired, BigInteger amountTokenMin, BigInteger amountETHMin, string to,
        BigInteger deadline, CancellationTokenSource cancellationToken = null)
    {
        var addLiquidityETHFunction = new AddLiquidityETHFunction
        {
            Token = token,
            AmountTokenDesired = amountTokenDesired,
            AmountTokenMin = amountTokenMin,
            AmountETHMin = amountETHMin,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(addLiquidityETHFunction, cancellationToken);
    }

    public Task<string> FactoryQueryAsync(FactoryFunction factoryFunction, BlockParameter blockParameter = null)
    {
        return ContractHandler.QueryAsync<FactoryFunction, string>(factoryFunction, blockParameter);
    }


    public Task<string> FactoryQueryAsync(BlockParameter blockParameter = null)
    {
        return ContractHandler.QueryAsync<FactoryFunction, string>(null, blockParameter);
    }

    public Task<BigInteger> GetAmountInQueryAsync(GetAmountInFunction getAmountInFunction,
        BlockParameter blockParameter = null)
    {
        return ContractHandler.QueryAsync<GetAmountInFunction, BigInteger>(getAmountInFunction, blockParameter);
    }


    public Task<BigInteger> GetAmountInQueryAsync(BigInteger amountOut, BigInteger reserveIn, BigInteger reserveOut,
        BlockParameter blockParameter = null)
    {
        var getAmountInFunction = new GetAmountInFunction
        {
            AmountOut = amountOut,
            ReserveIn = reserveIn,
            ReserveOut = reserveOut
        };

        return ContractHandler.QueryAsync<GetAmountInFunction, BigInteger>(getAmountInFunction, blockParameter);
    }

    public Task<BigInteger> GetAmountOutQueryAsync(GetAmountOutFunction getAmountOutFunction,
        BlockParameter blockParameter = null)
    {
        return ContractHandler.QueryAsync<GetAmountOutFunction, BigInteger>(getAmountOutFunction, blockParameter);
    }


    public Task<BigInteger> GetAmountOutQueryAsync(BigInteger amountIn, BigInteger reserveIn, BigInteger reserveOut,
        BlockParameter blockParameter = null)
    {
        var getAmountOutFunction = new GetAmountOutFunction
        {
            AmountIn = amountIn,
            ReserveIn = reserveIn,
            ReserveOut = reserveOut
        };

        return ContractHandler.QueryAsync<GetAmountOutFunction, BigInteger>(getAmountOutFunction, blockParameter);
    }

    public Task<List<BigInteger>> GetAmountsInQueryAsync(GetAmountsInFunction getAmountsInFunction,
        BlockParameter blockParameter = null)
    {
        return ContractHandler.QueryAsync<GetAmountsInFunction, List<BigInteger>>(getAmountsInFunction,
            blockParameter);
    }


    public Task<List<BigInteger>> GetAmountsInQueryAsync(BigInteger amountOut, List<string> path,
        BlockParameter blockParameter = null)
    {
        var getAmountsInFunction = new GetAmountsInFunction
        {
            AmountOut = amountOut,
            Path = path
        };

        return ContractHandler.QueryAsync<GetAmountsInFunction, List<BigInteger>>(getAmountsInFunction,
            blockParameter);
    }

    public Task<List<BigInteger>> GetAmountsOutQueryAsync(GetAmountsOutFunction getAmountsOutFunction,
        BlockParameter blockParameter = null)
    {
        return ContractHandler.QueryAsync<GetAmountsOutFunction, List<BigInteger>>(getAmountsOutFunction,
            blockParameter);
    }


    public Task<List<BigInteger>> GetAmountsOutQueryAsync(BigInteger amountIn, List<string> path,
        BlockParameter blockParameter = null)
    {
        var getAmountsOutFunction = new GetAmountsOutFunction
        {
            AmountIn = amountIn,
            Path = path
        };

        return ContractHandler.QueryAsync<GetAmountsOutFunction, List<BigInteger>>(getAmountsOutFunction,
            blockParameter);
    }

    public Task<BigInteger> QuoteQueryAsync(QuoteFunction quoteFunction, BlockParameter blockParameter = null)
    {
        return ContractHandler.QueryAsync<QuoteFunction, BigInteger>(quoteFunction, blockParameter);
    }


    public Task<BigInteger> QuoteQueryAsync(BigInteger amountA, BigInteger reserveA, BigInteger reserveB,
        BlockParameter blockParameter = null)
    {
        var quoteFunction = new QuoteFunction
        {
            AmountA = amountA,
            ReserveA = reserveA,
            ReserveB = reserveB
        };

        return ContractHandler.QueryAsync<QuoteFunction, BigInteger>(quoteFunction, blockParameter);
    }

    public Task<string> RemoveLiquidityRequestAsync(RemoveLiquidityFunction removeLiquidityFunction)
    {
        return ContractHandler.SendRequestAsync(removeLiquidityFunction);
    }

    public Task<TransactionReceipt> RemoveLiquidityRequestAndWaitForReceiptAsync(
        RemoveLiquidityFunction removeLiquidityFunction, CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(removeLiquidityFunction, cancellationToken);
    }

    public Task<string> RemoveLiquidityRequestAsync(string tokenA, string tokenB, BigInteger liquidity,
        BigInteger amountAMin, BigInteger amountBMin, string to, BigInteger deadline)
    {
        var removeLiquidityFunction = new RemoveLiquidityFunction
        {
            TokenA = tokenA,
            TokenB = tokenB,
            Liquidity = liquidity,
            AmountAMin = amountAMin,
            AmountBMin = amountBMin,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAsync(removeLiquidityFunction);
    }

    public Task<TransactionReceipt> RemoveLiquidityRequestAndWaitForReceiptAsync(string tokenA, string tokenB,
        BigInteger liquidity, BigInteger amountAMin, BigInteger amountBMin, string to, BigInteger deadline,
        CancellationTokenSource cancellationToken = null)
    {
        var removeLiquidityFunction = new RemoveLiquidityFunction
        {
            TokenA = tokenA,
            TokenB = tokenB,
            Liquidity = liquidity,
            AmountAMin = amountAMin,
            AmountBMin = amountBMin,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(removeLiquidityFunction, cancellationToken);
    }

    public Task<string> RemoveLiquidityETHRequestAsync(RemoveLiquidityETHFunction removeLiquidityETHFunction)
    {
        return ContractHandler.SendRequestAsync(removeLiquidityETHFunction);
    }

    public Task<TransactionReceipt> RemoveLiquidityETHRequestAndWaitForReceiptAsync(
        RemoveLiquidityETHFunction removeLiquidityETHFunction, CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(removeLiquidityETHFunction, cancellationToken);
    }

    public Task<string> RemoveLiquidityETHRequestAsync(string token, BigInteger liquidity,
        BigInteger amountTokenMin, BigInteger amountETHMin, string to, BigInteger deadline)
    {
        var removeLiquidityETHFunction = new RemoveLiquidityETHFunction
        {
            Token = token,
            Liquidity = liquidity,
            AmountTokenMin = amountTokenMin,
            AmountETHMin = amountETHMin,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAsync(removeLiquidityETHFunction);
    }

    public Task<TransactionReceipt> RemoveLiquidityETHRequestAndWaitForReceiptAsync(string token,
        BigInteger liquidity, BigInteger amountTokenMin, BigInteger amountETHMin, string to, BigInteger deadline,
        CancellationTokenSource cancellationToken = null)
    {
        var removeLiquidityETHFunction = new RemoveLiquidityETHFunction
        {
            Token = token,
            Liquidity = liquidity,
            AmountTokenMin = amountTokenMin,
            AmountETHMin = amountETHMin,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(removeLiquidityETHFunction, cancellationToken);
    }

    public Task<string> RemoveLiquidityETHSupportingFeeOnTransferTokensRequestAsync(
        RemoveLiquidityETHSupportingFeeOnTransferTokensFunction
            removeLiquidityETHSupportingFeeOnTransferTokensFunction)
    {
        return ContractHandler.SendRequestAsync(removeLiquidityETHSupportingFeeOnTransferTokensFunction);
    }

    public Task<TransactionReceipt> RemoveLiquidityETHSupportingFeeOnTransferTokensRequestAndWaitForReceiptAsync(
        RemoveLiquidityETHSupportingFeeOnTransferTokensFunction
            removeLiquidityETHSupportingFeeOnTransferTokensFunction,
        CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(
            removeLiquidityETHSupportingFeeOnTransferTokensFunction, cancellationToken);
    }

    public Task<string> RemoveLiquidityETHSupportingFeeOnTransferTokensRequestAsync(string token,
        BigInteger liquidity, BigInteger amountTokenMin, BigInteger amountETHMin, string to, BigInteger deadline)
    {
        var removeLiquidityETHSupportingFeeOnTransferTokensFunction =
            new RemoveLiquidityETHSupportingFeeOnTransferTokensFunction
            {
                Token = token,
                Liquidity = liquidity,
                AmountTokenMin = amountTokenMin,
                AmountETHMin = amountETHMin,
                To = to,
                Deadline = deadline
            };

        return ContractHandler.SendRequestAsync(removeLiquidityETHSupportingFeeOnTransferTokensFunction);
    }

    public Task<TransactionReceipt> RemoveLiquidityETHSupportingFeeOnTransferTokensRequestAndWaitForReceiptAsync(
        string token, BigInteger liquidity, BigInteger amountTokenMin, BigInteger amountETHMin, string to,
        BigInteger deadline, CancellationTokenSource cancellationToken = null)
    {
        var removeLiquidityETHSupportingFeeOnTransferTokensFunction =
            new RemoveLiquidityETHSupportingFeeOnTransferTokensFunction
            {
                Token = token,
                Liquidity = liquidity,
                AmountTokenMin = amountTokenMin,
                AmountETHMin = amountETHMin,
                To = to,
                Deadline = deadline
            };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(
            removeLiquidityETHSupportingFeeOnTransferTokensFunction, cancellationToken);
    }

    public Task<string> RemoveLiquidityETHWithPermitRequestAsync(
        RemoveLiquidityETHWithPermitFunction removeLiquidityETHWithPermitFunction)
    {
        return ContractHandler.SendRequestAsync(removeLiquidityETHWithPermitFunction);
    }

    public Task<TransactionReceipt> RemoveLiquidityETHWithPermitRequestAndWaitForReceiptAsync(
        RemoveLiquidityETHWithPermitFunction removeLiquidityETHWithPermitFunction,
        CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(removeLiquidityETHWithPermitFunction,
            cancellationToken);
    }

    public Task<string> RemoveLiquidityETHWithPermitRequestAsync(string token, BigInteger liquidity,
        BigInteger amountTokenMin, BigInteger amountETHMin, string to, BigInteger deadline, bool approveMax, byte v,
        byte[] r, byte[] s)
    {
        var removeLiquidityETHWithPermitFunction = new RemoveLiquidityETHWithPermitFunction
        {
            Token = token,
            Liquidity = liquidity,
            AmountTokenMin = amountTokenMin,
            AmountETHMin = amountETHMin,
            To = to,
            Deadline = deadline,
            ApproveMax = approveMax,
            V = v,
            R = r,
            S = s
        };

        return ContractHandler.SendRequestAsync(removeLiquidityETHWithPermitFunction);
    }

    public Task<TransactionReceipt> RemoveLiquidityETHWithPermitRequestAndWaitForReceiptAsync(string token,
        BigInteger liquidity, BigInteger amountTokenMin, BigInteger amountETHMin, string to, BigInteger deadline,
        bool approveMax, byte v, byte[] r, byte[] s, CancellationTokenSource cancellationToken = null)
    {
        var removeLiquidityETHWithPermitFunction = new RemoveLiquidityETHWithPermitFunction
        {
            Token = token,
            Liquidity = liquidity,
            AmountTokenMin = amountTokenMin,
            AmountETHMin = amountETHMin,
            To = to,
            Deadline = deadline,
            ApproveMax = approveMax,
            V = v,
            R = r,
            S = s
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(removeLiquidityETHWithPermitFunction,
            cancellationToken);
    }

    public Task<string> RemoveLiquidityETHWithPermitSupportingFeeOnTransferTokensRequestAsync(
        RemoveLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction
            removeLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction)
    {
        return ContractHandler.SendRequestAsync(removeLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction);
    }

    public Task<TransactionReceipt>
        RemoveLiquidityETHWithPermitSupportingFeeOnTransferTokensRequestAndWaitForReceiptAsync(
            RemoveLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction
                removeLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction,
            CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(
            removeLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction, cancellationToken);
    }

    public Task<string> RemoveLiquidityETHWithPermitSupportingFeeOnTransferTokensRequestAsync(string token,
        BigInteger liquidity, BigInteger amountTokenMin, BigInteger amountETHMin, string to, BigInteger deadline,
        bool approveMax, byte v, byte[] r, byte[] s)
    {
        var removeLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction =
            new RemoveLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction
            {
                Token = token,
                Liquidity = liquidity,
                AmountTokenMin = amountTokenMin,
                AmountETHMin = amountETHMin,
                To = to,
                Deadline = deadline,
                ApproveMax = approveMax,
                V = v,
                R = r,
                S = s
            };

        return ContractHandler.SendRequestAsync(removeLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction);
    }

    public Task<TransactionReceipt>
        RemoveLiquidityETHWithPermitSupportingFeeOnTransferTokensRequestAndWaitForReceiptAsync(string token,
            BigInteger liquidity, BigInteger amountTokenMin, BigInteger amountETHMin, string to,
            BigInteger deadline, bool approveMax, byte v, byte[] r, byte[] s,
            CancellationTokenSource cancellationToken = null)
    {
        var removeLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction =
            new RemoveLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction
            {
                Token = token,
                Liquidity = liquidity,
                AmountTokenMin = amountTokenMin,
                AmountETHMin = amountETHMin,
                To = to,
                Deadline = deadline,
                ApproveMax = approveMax,
                V = v,
                R = r,
                S = s
            };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(
            removeLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction, cancellationToken);
    }

    public Task<string> RemoveLiquidityWithPermitRequestAsync(
        RemoveLiquidityWithPermitFunction removeLiquidityWithPermitFunction)
    {
        return ContractHandler.SendRequestAsync(removeLiquidityWithPermitFunction);
    }

    public Task<TransactionReceipt> RemoveLiquidityWithPermitRequestAndWaitForReceiptAsync(
        RemoveLiquidityWithPermitFunction removeLiquidityWithPermitFunction,
        CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(removeLiquidityWithPermitFunction,
            cancellationToken);
    }

    public Task<string> RemoveLiquidityWithPermitRequestAsync(string tokenA, string tokenB, BigInteger liquidity,
        BigInteger amountAMin, BigInteger amountBMin, string to, BigInteger deadline, bool approveMax, byte v,
        byte[] r, byte[] s)
    {
        var removeLiquidityWithPermitFunction = new RemoveLiquidityWithPermitFunction
        {
            TokenA = tokenA,
            TokenB = tokenB,
            Liquidity = liquidity,
            AmountAMin = amountAMin,
            AmountBMin = amountBMin,
            To = to,
            Deadline = deadline,
            ApproveMax = approveMax,
            V = v,
            R = r,
            S = s
        };

        return ContractHandler.SendRequestAsync(removeLiquidityWithPermitFunction);
    }

    public Task<TransactionReceipt> RemoveLiquidityWithPermitRequestAndWaitForReceiptAsync(string tokenA,
        string tokenB, BigInteger liquidity, BigInteger amountAMin, BigInteger amountBMin, string to,
        BigInteger deadline, bool approveMax, byte v, byte[] r, byte[] s,
        CancellationTokenSource cancellationToken = null)
    {
        var removeLiquidityWithPermitFunction = new RemoveLiquidityWithPermitFunction
        {
            TokenA = tokenA,
            TokenB = tokenB,
            Liquidity = liquidity,
            AmountAMin = amountAMin,
            AmountBMin = amountBMin,
            To = to,
            Deadline = deadline,
            ApproveMax = approveMax,
            V = v,
            R = r,
            S = s
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(removeLiquidityWithPermitFunction,
            cancellationToken);
    }

    public Task<string> SwapETHForExactTokensRequestAsync(
        SwapETHForExactTokensFunction swapETHForExactTokensFunction)
    {
        return ContractHandler.SendRequestAsync(swapETHForExactTokensFunction);
    }

    public Task<TransactionReceipt> SwapETHForExactTokensRequestAndWaitForReceiptAsync(
        SwapETHForExactTokensFunction swapETHForExactTokensFunction,
        CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(swapETHForExactTokensFunction, cancellationToken);
    }

    public Task<string> SwapETHForExactTokensRequestAsync(BigInteger amountOut, List<string> path, string to,
        BigInteger deadline)
    {
        var swapETHForExactTokensFunction = new SwapETHForExactTokensFunction
        {
            AmountOut = amountOut,
            Path = path,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAsync(swapETHForExactTokensFunction);
    }

    public Task<TransactionReceipt> SwapETHForExactTokensRequestAndWaitForReceiptAsync(BigInteger amountOut,
        List<string> path, string to, BigInteger deadline, CancellationTokenSource cancellationToken = null)
    {
        var swapETHForExactTokensFunction = new SwapETHForExactTokensFunction
        {
            AmountOut = amountOut,
            Path = path,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(swapETHForExactTokensFunction, cancellationToken);
    }

    public Task<string> SwapExactETHForTokensRequestAsync(
        SwapExactETHForTokensFunction swapExactETHForTokensFunction)
    {
        return ContractHandler.SendRequestAsync(swapExactETHForTokensFunction);
    }

    public Task<TransactionReceipt> SwapExactETHForTokensRequestAndWaitForReceiptAsync(
        SwapExactETHForTokensFunction swapExactETHForTokensFunction,
        CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(swapExactETHForTokensFunction, cancellationToken);
    }

    public Task<string> SwapExactETHForTokensRequestAsync(BigInteger amountOutMin, List<string> path, string to,
        BigInteger deadline)
    {
        var swapExactETHForTokensFunction = new SwapExactETHForTokensFunction
        {
            AmountOutMin = amountOutMin,
            Path = path,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAsync(swapExactETHForTokensFunction);
    }

    public Task<TransactionReceipt> SwapExactETHForTokensRequestAndWaitForReceiptAsync(BigInteger amountOutMin,
        List<string> path, string to, BigInteger deadline, CancellationTokenSource cancellationToken = null)
    {
        var swapExactETHForTokensFunction = new SwapExactETHForTokensFunction
        {
            AmountOutMin = amountOutMin,
            Path = path,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(swapExactETHForTokensFunction, cancellationToken);
    }

    public Task<string> SwapExactETHForTokensSupportingFeeOnTransferTokensRequestAsync(
        SwapExactETHForTokensSupportingFeeOnTransferTokensFunction
            swapExactETHForTokensSupportingFeeOnTransferTokensFunction)
    {
        return ContractHandler.SendRequestAsync(swapExactETHForTokensSupportingFeeOnTransferTokensFunction);
    }

    public Task<TransactionReceipt> SwapExactETHForTokensSupportingFeeOnTransferTokensRequestAndWaitForReceiptAsync(
        SwapExactETHForTokensSupportingFeeOnTransferTokensFunction
            swapExactETHForTokensSupportingFeeOnTransferTokensFunction,
        CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(
            swapExactETHForTokensSupportingFeeOnTransferTokensFunction, cancellationToken);
    }

    public Task<string> SwapExactETHForTokensSupportingFeeOnTransferTokensRequestAsync(BigInteger amountOutMin,
        List<string> path, string to, BigInteger deadline)
    {
        var swapExactETHForTokensSupportingFeeOnTransferTokensFunction =
            new SwapExactETHForTokensSupportingFeeOnTransferTokensFunction
            {
                AmountOutMin = amountOutMin,
                Path = path,
                To = to,
                Deadline = deadline
            };

        return ContractHandler.SendRequestAsync(swapExactETHForTokensSupportingFeeOnTransferTokensFunction);
    }

    public Task<TransactionReceipt> SwapExactETHForTokensSupportingFeeOnTransferTokensRequestAndWaitForReceiptAsync(
        BigInteger amountOutMin, List<string> path, string to, BigInteger deadline,
        CancellationTokenSource cancellationToken = null)
    {
        var swapExactETHForTokensSupportingFeeOnTransferTokensFunction =
            new SwapExactETHForTokensSupportingFeeOnTransferTokensFunction
            {
                AmountOutMin = amountOutMin,
                Path = path,
                To = to,
                Deadline = deadline
            };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(
            swapExactETHForTokensSupportingFeeOnTransferTokensFunction, cancellationToken);
    }

    public Task<string> SwapExactTokensForETHRequestAsync(
        SwapExactTokensForETHFunction swapExactTokensForETHFunction)
    {
        return ContractHandler.SendRequestAsync(swapExactTokensForETHFunction);
    }

    public Task<TransactionReceipt> SwapExactTokensForETHRequestAndWaitForReceiptAsync(
        SwapExactTokensForETHFunction swapExactTokensForETHFunction,
        CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(swapExactTokensForETHFunction, cancellationToken);
    }

    public Task<string> SwapExactTokensForETHRequestAsync(BigInteger amountIn, BigInteger amountOutMin,
        List<string> path, string to, BigInteger deadline)
    {
        var swapExactTokensForETHFunction = new SwapExactTokensForETHFunction
        {
            AmountIn = amountIn,
            AmountOutMin = amountOutMin,
            Path = path,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAsync(swapExactTokensForETHFunction);
    }

    public Task<TransactionReceipt> SwapExactTokensForETHRequestAndWaitForReceiptAsync(BigInteger amountIn,
        BigInteger amountOutMin, List<string> path, string to, BigInteger deadline,
        CancellationTokenSource cancellationToken = null)
    {
        var swapExactTokensForETHFunction = new SwapExactTokensForETHFunction
        {
            AmountIn = amountIn,
            AmountOutMin = amountOutMin,
            Path = path,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(swapExactTokensForETHFunction, cancellationToken);
    }

    public Task<string> SwapExactTokensForETHSupportingFeeOnTransferTokensRequestAsync(
        SwapExactTokensForETHSupportingFeeOnTransferTokensFunction
            swapExactTokensForETHSupportingFeeOnTransferTokensFunction)
    {
        return ContractHandler.SendRequestAsync(swapExactTokensForETHSupportingFeeOnTransferTokensFunction);
    }

    public Task<TransactionReceipt> SwapExactTokensForETHSupportingFeeOnTransferTokensRequestAndWaitForReceiptAsync(
        SwapExactTokensForETHSupportingFeeOnTransferTokensFunction
            swapExactTokensForETHSupportingFeeOnTransferTokensFunction,
        CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(
            swapExactTokensForETHSupportingFeeOnTransferTokensFunction, cancellationToken);
    }

    public Task<string> SwapExactTokensForETHSupportingFeeOnTransferTokensRequestAsync(BigInteger amountIn,
        BigInteger amountOutMin, List<string> path, string to, BigInteger deadline)
    {
        var swapExactTokensForETHSupportingFeeOnTransferTokensFunction =
            new SwapExactTokensForETHSupportingFeeOnTransferTokensFunction
            {
                AmountIn = amountIn,
                AmountOutMin = amountOutMin,
                Path = path,
                To = to,
                Deadline = deadline
            };

        return ContractHandler.SendRequestAsync(swapExactTokensForETHSupportingFeeOnTransferTokensFunction);
    }

    public Task<TransactionReceipt> SwapExactTokensForETHSupportingFeeOnTransferTokensRequestAndWaitForReceiptAsync(
        BigInteger amountIn, BigInteger amountOutMin, List<string> path, string to, BigInteger deadline,
        CancellationTokenSource cancellationToken = null)
    {
        var swapExactTokensForETHSupportingFeeOnTransferTokensFunction =
            new SwapExactTokensForETHSupportingFeeOnTransferTokensFunction
            {
                AmountIn = amountIn,
                AmountOutMin = amountOutMin,
                Path = path,
                To = to,
                Deadline = deadline
            };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(
            swapExactTokensForETHSupportingFeeOnTransferTokensFunction, cancellationToken);
    }

    public Task<string> SwapExactTokensForTokensRequestAsync(
        SwapExactTokensForTokensFunction swapExactTokensForTokensFunction)
    {
        return ContractHandler.SendRequestAsync(swapExactTokensForTokensFunction);
    }

    public Task<TransactionReceipt> SwapExactTokensForTokensRequestAndWaitForReceiptAsync(
        SwapExactTokensForTokensFunction swapExactTokensForTokensFunction,
        CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(swapExactTokensForTokensFunction,
            cancellationToken);
    }

    public Task<string> SwapExactTokensForTokensRequestAsync(BigInteger amountIn, BigInteger amountOutMin,
        List<string> path, string to, BigInteger deadline)
    {
        var swapExactTokensForTokensFunction = new SwapExactTokensForTokensFunction
        {
            AmountIn = amountIn,
            AmountOutMin = amountOutMin,
            Path = path,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAsync(swapExactTokensForTokensFunction);
    }

    public Task<TransactionReceipt> SwapExactTokensForTokensRequestAndWaitForReceiptAsync(BigInteger amountIn,
        BigInteger amountOutMin, List<string> path, string to, BigInteger deadline,
        CancellationTokenSource cancellationToken = null)
    {
        var swapExactTokensForTokensFunction = new SwapExactTokensForTokensFunction
        {
            AmountIn = amountIn,
            AmountOutMin = amountOutMin,
            Path = path,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(swapExactTokensForTokensFunction,
            cancellationToken);
    }

    public Task<string> SwapExactTokensForTokensSupportingFeeOnTransferTokensRequestAsync(
        SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction
            swapExactTokensForTokensSupportingFeeOnTransferTokensFunction)
    {
        return ContractHandler.SendRequestAsync(swapExactTokensForTokensSupportingFeeOnTransferTokensFunction);
    }

    public Task<TransactionReceipt>
        SwapExactTokensForTokensSupportingFeeOnTransferTokensRequestAndWaitForReceiptAsync(
            SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction
                swapExactTokensForTokensSupportingFeeOnTransferTokensFunction,
            CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(
            swapExactTokensForTokensSupportingFeeOnTransferTokensFunction, cancellationToken);
    }

    public Task<string> SwapExactTokensForTokensSupportingFeeOnTransferTokensRequestAsync(BigInteger amountIn,
        BigInteger amountOutMin, List<string> path, string to, BigInteger deadline)
    {
        var swapExactTokensForTokensSupportingFeeOnTransferTokensFunction =
            new SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction
            {
                AmountIn = amountIn,
                AmountOutMin = amountOutMin,
                Path = path,
                To = to,
                Deadline = deadline
            };

        return ContractHandler.SendRequestAsync(swapExactTokensForTokensSupportingFeeOnTransferTokensFunction);
    }

    public Task<TransactionReceipt>
        SwapExactTokensForTokensSupportingFeeOnTransferTokensRequestAndWaitForReceiptAsync(BigInteger amountIn,
            BigInteger amountOutMin, List<string> path, string to, BigInteger deadline,
            CancellationTokenSource cancellationToken = null)
    {
        var swapExactTokensForTokensSupportingFeeOnTransferTokensFunction =
            new SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction
            {
                AmountIn = amountIn,
                AmountOutMin = amountOutMin,
                Path = path,
                To = to,
                Deadline = deadline
            };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(
            swapExactTokensForTokensSupportingFeeOnTransferTokensFunction, cancellationToken);
    }

    public Task<string> SwapTokensForExactETHRequestAsync(
        SwapTokensForExactETHFunction swapTokensForExactETHFunction)
    {
        return ContractHandler.SendRequestAsync(swapTokensForExactETHFunction);
    }

    public Task<TransactionReceipt> SwapTokensForExactETHRequestAndWaitForReceiptAsync(
        SwapTokensForExactETHFunction swapTokensForExactETHFunction,
        CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(swapTokensForExactETHFunction, cancellationToken);
    }

    public Task<string> SwapTokensForExactETHRequestAsync(BigInteger amountOut, BigInteger amountInMax,
        List<string> path, string to, BigInteger deadline)
    {
        var swapTokensForExactETHFunction = new SwapTokensForExactETHFunction
        {
            AmountOut = amountOut,
            AmountInMax = amountInMax,
            Path = path,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAsync(swapTokensForExactETHFunction);
    }

    public Task<TransactionReceipt> SwapTokensForExactETHRequestAndWaitForReceiptAsync(BigInteger amountOut,
        BigInteger amountInMax, List<string> path, string to, BigInteger deadline,
        CancellationTokenSource cancellationToken = null)
    {
        var swapTokensForExactETHFunction = new SwapTokensForExactETHFunction
        {
            AmountOut = amountOut,
            AmountInMax = amountInMax,
            Path = path,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(swapTokensForExactETHFunction, cancellationToken);
    }

    public Task<string> SwapTokensForExactTokensRequestAsync(
        SwapTokensForExactTokensFunction swapTokensForExactTokensFunction)
    {
        return ContractHandler.SendRequestAsync(swapTokensForExactTokensFunction);
    }

    public Task<TransactionReceipt> SwapTokensForExactTokensRequestAndWaitForReceiptAsync(
        SwapTokensForExactTokensFunction swapTokensForExactTokensFunction,
        CancellationTokenSource cancellationToken = null)
    {
        return ContractHandler.SendRequestAndWaitForReceiptAsync(swapTokensForExactTokensFunction,
            cancellationToken);
    }

    public Task<string> SwapTokensForExactTokensRequestAsync(BigInteger amountOut, BigInteger amountInMax,
        List<string> path, string to, BigInteger deadline)
    {
        var swapTokensForExactTokensFunction = new SwapTokensForExactTokensFunction
        {
            AmountOut = amountOut,
            AmountInMax = amountInMax,
            Path = path,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAsync(swapTokensForExactTokensFunction);
    }

    public Task<TransactionReceipt> SwapTokensForExactTokensRequestAndWaitForReceiptAsync(BigInteger amountOut,
        BigInteger amountInMax, List<string> path, string to, BigInteger deadline,
        CancellationTokenSource cancellationToken = null)
    {
        var swapTokensForExactTokensFunction = new SwapTokensForExactTokensFunction
        {
            AmountOut = amountOut,
            AmountInMax = amountInMax,
            Path = path,
            To = to,
            Deadline = deadline
        };

        return ContractHandler.SendRequestAndWaitForReceiptAsync(swapTokensForExactTokensFunction,
            cancellationToken);
    }
}