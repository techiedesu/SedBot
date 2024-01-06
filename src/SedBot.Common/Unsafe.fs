module SedBot.Common.Unsafe

#nowarn "42"

let zeroCreateUnsafe<'T> (count: int) = (# "newarr !0" count : 'T[] #)
