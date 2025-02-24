﻿using Mono.Cecil;
using System;
using System.Collections.Generic;
using Weaver.Contracts.Diagnostics;

namespace Weaver.Contracts
{
    /// <summary>
    /// The contract between an assembly and the weaving process. 
    /// </summary>
    public interface IAssemblyWeaver
    {
        /// <summary>
        /// Gets or sets the working directory.
        /// </summary>
        string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets the logger that we output too
        /// </summary>
        ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the assembly cache.
        /// </summary>
        IAssemblyCache AssemblyCache { get; }

        /// <summary>
        /// Weaves an assembly from disk.
        /// </summary>
        /// <param name="assemblyPath">The system path to the assembly.</param>
        /// <param name="addIns">The add ins youu would like to run.</param>
        /// <returns>True if it's successful and false if it's not.</returns>
        bool WeaveAssembly(string assemblyPath, IEnumerable<IWeaverAddin> addIns);

        /// <summary>
        /// Weaves an assembly from disk.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="addIns">The add ins youu would like to run.</param>
        /// <returns>True if it's successful and false if it's not.</returns>
        bool WeaveAssembly(string assemblyPath, IEnumerable<Type> addIns);
    }
}
