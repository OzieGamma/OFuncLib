// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AttemptTest.fs" company="Oswald Maskens">
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
module OFuncLib.Test.AttemptTest

open OFuncLib
open NUnit.Framework

[<Test>]
let ``Attempt.ofOption should work``() = 
    [ Some 42, Ok 42
      None, Fail [ "msg" ] ]
    |> testOnDataMap (Attempt.ofOption [ "msg" ])

[<Test>]
let ``Attempt.get should return on Ok``() = 
    Ok 42
    |> Attempt.get id
    |> shouldEq 42

[<Test>]
let ``Attempt.orElse should work``() = 
    Ok 42
    |> Attempt.orElse 56
    |> shouldEq 42
    Fail []
    |> Attempt.orElse 56
    |> shouldEq 56

[<Test>]
let ``Attempt.map should work``() = 
    [ Ok 42, Ok 84
      Fail [ "msg" ], Fail [ "msg" ] ]
    |> testOnDataMap (Attempt.map (fun x -> x * 2))

[<Test>]
let ``Attempt.lift should work``() = 
    [ Fail [ "msg" ], Fail [ "msg" ]
      Ok(Fail [ "msg" ]), Fail [ "msg" ]
      Ok(Ok 42), Ok 42 ]
    |> testOnDataMap Attempt.lift

[<Test>]
let ``Attempt.bind should return Fail on Fail``() = 
    Fail [ "a" ]
    |> Attempt.bind (fun x -> Ok x)
    |> shouldEq (Fail [ "a" ])

[<Test>]
let ``Attempt.bind should return Fail on Ok Fail``() = 
    Ok 42
    |> Attempt.bind (fun _ -> Fail [ "msg" ])
    |> shouldEq (Fail [ "msg" ])

[<Test>]
let ``Attempt.bind should return Ok on Ok``() = 
    Ok 42
    |> Attempt.bind (fun x -> Ok(x * 2))
    |> shouldEq (Ok 84)

[<Test>]
let ``Attempt.liftList should work``() = 
    [ [ Ok 42
        Ok 5 ], Ok [ 42; 5 ]
      [ Ok 42
        Fail [ "1"; "2" ]
        Ok 69
        Fail [ "3" ]
        Fail [ "4"; "5"; "6" ] ], Fail [ "1"; "2"; "3"; "4"; "5"; "6" ] ]
    |> testOnDataMap Attempt.liftList

[<Test>]
let ``Attempt.lift2 should work``() = 
    [ (Fail [ "1" ], Fail [ "2" ]), Fail [ "1"; "2" ]
      (Fail [ "1" ], Ok 42), Fail [ "1" ]
      (Ok 2, Fail [ "1" ]), Fail [ "1" ]
      (Ok 2, Ok 42), Ok(2, 42) ]
    |> testOnDataMap Attempt.lift2

[<Test>]
let ``Attempt.lift3 should work``() = 
    [ (Fail [ "1" ], Fail [ "2" ], Fail [ "3" ]), Fail [ "1"; "2"; "3" ]
      (Fail [ "1" ], Ok 42, Fail [ "2" ]), Fail [ "1"; "2" ]
      (Ok 2, Fail [ "1"; "2" ], Fail [ "3" ]), Fail [ "1"; "2"; "3" ]
      (Ok 2, Ok 42, Fail [ "1" ]), Fail [ "1" ]
      (Fail [ "1"; "2" ], Fail [ "3"; "4"; "5" ], Ok 1337), Fail [ "1"; "2"; "3"; "4"; "5" ]
      (Fail [ "1" ], Ok 42, Ok 1337), Fail [ "1" ]
      (Ok 2, Fail [ "1" ], Ok 1337), Fail [ "1" ]
      (Ok 2, Ok 42, Ok 1337), Ok(2, 42, 1337) ]
    |> testOnDataMap Attempt.lift3

[<Test>]
let ``Attempt.mapFail should work``() = 
    [ Ok 42, Ok 42
      Fail [ "msg1"; "msg2" ], Fail [ "42msg1"; "42msg2" ] ]
    |> testOnDataMap (Attempt.mapFail (sprintf "42%s"))
