// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SeqExt.fs" company="Oswald Maskens">
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

type MutableList<'T> = System.Collections.Generic.List<'T>

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module SeqExt = 
    let private optionalHead (list : List<'T>) = 
        if list.IsEmpty then None
        else Some list.Head
    
    let public splitWithLookAhead (splitter : 'T -> Option<'T> -> bool) (list : #seq<'T>) : List<List<'T>> = 
        let mutable inp = list |> List.ofSeq
        let res = new MutableList<List<'T>>()
        let acc = new MutableList<'T>()
        while not inp.IsEmpty do
            let head = inp.Head
            inp <- inp.Tail
            if splitter head (optionalHead inp) then 
                res.Add(acc |> List.ofSeq)
                acc.Clear()
            acc.Add(head)
        res.Add(acc |> List.ofSeq)
        res
        |> List.ofSeq
        |> List.filter (fun l -> not l.IsEmpty)
    
    let public split (splitter : 'T -> bool) (list : #seq<'T>) : List<List<'T>> = list |> splitWithLookAhead (fun head _ -> splitter head)
