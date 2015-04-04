﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestHelpersTest.fs" company="Oswald Maskens">
//   Copyright 2014 Oswald Maskens
//   
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
module OFuncLib.Test.TestHelpersTest

open OFuncLib
open NUnit.Framework

[<Test>]
let ``TestHelpers.shouldPass should do nothing onpass``() = 
    Pass |> shouldPass

[<Test>]
let ``TestHelpers.shouldPass should throw on pass``() =
    try
        Failure [] |> shouldPass
    with
    | :? AssertionException -> ()
    | _ -> Assert.Fail "Should throw assertion error"