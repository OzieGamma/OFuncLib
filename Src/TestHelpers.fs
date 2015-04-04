// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestHelpers.fs" company="Oswald Maskens">
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

namespace OFuncLib

/// <sumary>
/// The result of a test, either a pass or a fail
/// </sumary>
type TestResult = 
    | Pass
    | Failure of List<string>

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module TestResult = 
    let liftList (list : List<TestResult>) : TestResult = 
        let iter errors elem = 
            match elem with
            | Pass -> errors
            | Failure msg -> msg :: errors
        
        let errors = list |> List.fold iter []
        if errors.IsEmpty then Pass
        else 
            Failure(errors
                    |> List.rev
                    |> List.concat)
[<AutoOpen>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module TestHelper = 
    open NUnit.Framework

    let private mkStringFromList (list : List<string>) : string = 
        (sprintf "%i failures" list.Length) + (list |> List.fold (fun msg elem -> msg + System.Environment.NewLine + elem) "")
    
    
    let public shouldPass (actual : TestResult) : unit = 
        match actual with
        | Pass -> Assert.IsTrue true
        | Failure msg -> Assert.Fail(msg |> mkStringFromList)

    
    let public shouldEq<'T when 'T : equality> (expected : 'T) (actual : 'T) : unit = Assert.AreEqual(expected, actual)

    
    let public shouldFail (actual : GenericAttempt<'TOk, 'TFail>) : unit = 
        match actual with
        | Ok _ -> Assert.Fail()
        | Fail _ -> Assert.IsTrue true

    
    let public shouldBeOk (actual : GenericAttempt<'TOk, string>) : unit = 
        match actual with
        | Ok _ -> Assert.IsTrue true
        | Fail msg -> Assert.Fail(msg |> mkStringFromList)

    
    let public testOnData (f : 'T -> TestResult) (data : List<'T>) : unit = 
        data
        |> List.map f
        |> TestResult.liftList
        |> shouldPass

    let inline private fail input expected actual = 
        Failure [ sprintf "%A %s did not equal expected %A%s input: %A" actual System.Environment.NewLine expected System.Environment.NewLine input ]

    let public tryEq<'TInp, 'T when 'T : equality> (input : 'TInp) (expected : 'T) (actual : 'T) : TestResult = 
        if expected = actual then Pass
        else fail input expected actual

    let public testOnDataShouldFail (f : 'A -> GenericAttempt<'TOk, 'TFail>) (data : List<'A>) : unit = 
        data |> testOnData (fun input -> 
                    let res = f input
                    match res with
                    | Ok _ -> Failure [ sprintf "Did not fail with input %A" input ]
                    | Fail _ -> Pass)

    let public testOnDataMap (f : 'A -> 'B) (data : List<'A * 'B>) : unit = data |> testOnData (fun (a, b) -> f a |> tryEq a b)
    
    let public testOnDataMapAttempt (f : 'A -> GenericAttempt<'B, 'TFail>) (data : List<'A * 'B>) : unit = 
        data |> testOnData (fun (a, b) -> f a |> tryEq a (Ok b))

    let public testOnDataLoop (aToB : 'A -> GenericAttempt<'B, 'TFail>) (bToA : 'B -> GenericAttempt<'A, 'TFail>) (data : List<'A>) : unit = 
        data |> testOnData (fun inp -> 
                    inp
                    |> aToB
                    |> Attempt.bind bToA
                    |> tryEq inp (Ok inp))