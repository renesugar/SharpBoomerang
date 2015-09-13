﻿namespace SharpBoomerang

open System
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.DerivedPatterns

/// Represents a bidirectional data source, supporting the Read and Write operations.
type IChannel<'t> =
    /// Reads this `IChannel` and calls the passed function with the result.
    abstract Read : ('t -> unit) -> unit

    /// Writes a`'t` into this `IChannel`.
    /// If this instance represents a read-only channel, then this method must be a no-op.
    abstract Write : 't -> unit

/// A read-only channel that is initialized with a constant value.
type ConstChannel<'t>(value : 't) =
    interface IChannel<'t> with
        member x.Read ret = ret value
        member x.Write v  = ()

/// A channel for directly accessing a mutable data source, such as a
///  mutable `let` binding, or a read-write property.
type ExprChannel<'t>(expr : Expr<'t>) =
    static let rec getExpr = function
    | Value(value, _) -> value
    | PropertyGet(None, pi, _) -> pi.GetValue(null, null)
    | PropertyGet(Some(expr), pi, _) -> pi.GetValue(getExpr expr, null)
    | u -> failwithf "Unsupported Expr: %A" u
    static let setExpr v = function
    | Value(value, _) -> () // do nothing so behavior matches ConstChannel
    | PropertyGet(None, pi, _) -> pi.SetValue(null, v, null)
    | PropertyGet(Some(expr), pi, _) -> pi.SetValue(getExpr expr, v, null)
    | u -> failwithf "Unsupported Expr: %A" u
    interface IChannel<'t> with
        member x.Read ret = getExpr expr :?> 't |> ret
        member x.Write v  = setExpr v expr

/// A channel for piping values between boomerangs. If the channel is read
///  and there is no value available, the callback will be saved and called
///  on the next `Write` operation.
type PipeChannel<'t>(initialValue : 't option) =
    let mutable value = initialValue
    let mutable valueCallback = Unchecked.defaultof<Action<'t>>
    new() = PipeChannel(None)
    member x.Value = value
    interface IChannel<'t> with
        member x.Read ret =
            match value with
            | Some v -> ret v
            | None   ->
                valueCallback <- Delegate.Combine(valueCallback, Action<'t>(ret)) :?> _
        member x.Write v  =
            value <- Some v
            match valueCallback with
            | null     -> ()
            | callback ->
                callback.Invoke(v)
                valueCallback <- null

// Maps one channel type onto another
type MapChannel<'t1,'t2>(src : IChannel<'t1>, t1tot2 : 't1 -> 't2, t2tot1 : 't2 -> 't1) =
    interface IChannel<'t2> with
        member x.Read ret = src.Read(fun v -> v |> t1tot2 |> ret)
        member x.Write v  = src.Write(v |> t2tot1)
