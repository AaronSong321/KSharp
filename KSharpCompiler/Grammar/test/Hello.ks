using System
using System.Collections.Generic


namespace Jmas

class Program {
    static fun Main() {
        const message = "Hello world!"
        mutable i = 15
        System.Console.WriteLine("{message} {if! message {3} else {i}:g}!")
    }
}

infix fun To(start: int, end: int): IEnumerable {
    yield 1
    yield 2
    yield 3
}

