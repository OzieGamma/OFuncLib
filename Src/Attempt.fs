// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Attempt.fs" company="Oswald Maskens">
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
/// An attempt at something. Like an option but keeps a list of errors when we fail.
/// </sumary>
[<CompiledName("Attempt")>]
type public GenericAttempt<'TOk, 'TFail> = 
    | Ok of 'TOk
    | Fail of List<'TFail>

type public Attempt<'T> = GenericAttempt<'T, string>

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module Attempt = 
    let public ofOption (msg : List<'TFail>) (option : Option<'TOk>) : GenericAttempt<'TOk, 'TFail> = 
        match option with
        | Some v -> Ok v
        | None -> Fail msg
    
    let public get (formatter : 'TFail -> string) (attempt : GenericAttempt<'TOk, 'TFail>) : 'TOk = 
        match attempt with
        | Ok v -> v
        | Fail msg -> failwith (msg |> List.fold (fun out entry -> out + System.Environment.NewLine + (formatter entry)) "")
    
    let public orElse (otherwise : 'TOk) (attempt : GenericAttempt<'TOk, 'TFail>) : 'TOk = 
        match attempt with
        | Ok v -> v
        | Fail _ -> otherwise
    
    (* Monadic operations *)
    let public map (f : 'A -> 'B) (attempt : GenericAttempt<'A, 'TFail>) : GenericAttempt<'B, 'TFail> = 
        match attempt with
        | Ok v -> Ok(f v)
        | Fail msg -> Fail msg
    
    let public lift (attempt : GenericAttempt<GenericAttempt<'TOk, 'TFail>, 'TFail>) : GenericAttempt<'TOk, 'TFail> = 
        match attempt with
        | Ok v -> v
        | Fail msg -> Fail msg
    
    let public bind (f : 'A -> GenericAttempt<'B, 'TFail>) (attempt : GenericAttempt<'A, 'TFail>) : GenericAttempt<'B, 'TFail> = 
        attempt
        |> map f
        |> lift
    
    (* Methods for nice lift & map on lists and tuples *)
    let public liftList (list : List<GenericAttempt<'TOk, 'TFail>>) : GenericAttempt<List<'TOk>, 'TFail> = 
        let iter (errors, values) elem = 
            match elem with
            | Ok v -> (errors, v :: values)
            | Fail msg -> (msg :: errors, values)
        
        let (errors, values) = list |> List.fold iter ([], [])
        if errors.IsEmpty then Ok(values |> List.rev)
        else 
            Fail(errors
                 |> List.rev
                 |> List.concat)
    
    let public lift2 (tuple : GenericAttempt<'A, 'TFail> * GenericAttempt<'B, 'TFail>) : GenericAttempt<'A * 'B, 'TFail> = 
        match tuple with
        | Ok valA, Ok valB -> Ok(valA, valB)
        | Ok _, Fail msgB -> Fail msgB
        | Fail msgA, Ok _ -> Fail msgA
        | Fail msgA, Fail msgB -> Fail(List.concat [ msgA; msgB ])
    
    let public lift3 (tuple : GenericAttempt<'A, 'TFail> * GenericAttempt<'B, 'TFail> * GenericAttempt<'C, 'TFail>) : GenericAttempt<'A * 'B * 'C, 'TFail> = 
        match tuple with
        | Ok valA, Ok valB, Ok valC -> Ok(valA, valB, valC)
        | Ok _, Ok _, Fail msgC -> Fail msgC
        | Ok _, Fail msgB, Ok _ -> Fail msgB
        | Ok _, Fail msgB, Fail msgC -> Fail(List.concat [ msgB; msgC ])
        | Fail msgA, Ok _, Ok _ -> Fail msgA
        | Fail msgA, Ok _, Fail msgC -> Fail(List.concat [ msgA; msgC ])
        | Fail msgA, Fail msgB, Ok _ -> Fail(List.concat [ msgA; msgB ])
        | Fail msgA, Fail msgB, Fail msgC -> Fail(List.concat [ msgA; msgB; msgC ])
    
    (* Helpers for formatting failures *)
    let public mapFail (formatter : 'TFailA -> 'TFailB) (attempt : GenericAttempt<'A, 'TFailA>) : GenericAttempt<'A, 'TFailB> = 
        match attempt with
        | Ok v -> Ok v
        | Fail msg -> Fail(msg |> List.map formatter)
    
    (* helpers with tuples and maps *)
    let public lift2curriedMap (f : 'A -> 'B -> 'TRes) (tuple : GenericAttempt<'A, 'TFail> * GenericAttempt<'B, 'TFail>) : GenericAttempt<'TRes, 'TFail> = 
        tuple
        |> lift2
        |> map (fun (a, b) -> f a b)
    
    let public lift3curriedMap (f : 'A -> 'B -> 'C -> 'TRes) (tuple : GenericAttempt<'A, 'TFail> * GenericAttempt<'B, 'TFail> * GenericAttempt<'C, 'TFail>) : GenericAttempt<'TRes, 'TFail> = 
        tuple
        |> lift3
        |> map (fun (a, b, c) -> f a b c)
    
    let public lift2tupleMap (f : 'A * 'B -> 'TRes) (tuple : GenericAttempt<'A, 'TFail> * GenericAttempt<'B, 'TFail>) : GenericAttempt<'TRes, 'TFail> = 
        tuple
        |> lift2
        |> map f
    
    let public lift3tupleMap (f : 'A * 'B * 'C -> 'TRes) (tuple : GenericAttempt<'A, 'TFail> * GenericAttempt<'B, 'TFail> * GenericAttempt<'C, 'TFail>) : GenericAttempt<'TRes, 'TFail> = 
        tuple
        |> lift3
        |> map f
