using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Scripts.Runtime.Core.Modules
{
    public class ModulesFacade 
    {
        private readonly IReadOnlyList<IBootstrapModule> _modules;
        private readonly Dictionary<Type, IBootstrapModule> _bootstrapModulesMap;
        
        public ModulesFacade(IReadOnlyList<IBootstrapModule> modules)
        {
            _modules = modules;
            _bootstrapModulesMap = _modules.ToDictionary(x => x.GetType(), x => x);
        }
        
        public SteamModule GetSteamModule() => _bootstrapModulesMap[typeof(SteamModule)] as SteamModule;

        public async UniTask Load()
        {
            foreach (var bootstrapModule in _modules)
            {
                await bootstrapModule.InitializeAsync(Application.exitCancellationToken);
            }
        }
        
    }
}