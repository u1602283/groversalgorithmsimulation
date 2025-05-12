# Grover's Algorithm Simulation

Got inspired after learning some very basic quantum computing theory.

## What?

Simulates the operation of Grover's algorithm (the quantum search algorithm) for an arbitrary number of qubits.

Outputs the percentage of runs that produced the correct answer for each number of qubits tested.

As this is running on a traditional computer, the Oracle operator has a precomputed marked state, rather than using some quantum logic to flip the amplitude for the marked state(s).

## To run

Requires .NET8.0.

Modify the constants at the top of [Program.cs](./GroversAlgorithmSimulation/Program.cs) - values of `MaxNQubits` above 10 slow down very quickly! 

Run with:

```
dotnet run
```
