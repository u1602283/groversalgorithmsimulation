namespace GroversAlgorithmSimulation;

public static class Program
{
    // A greater number of qubits will give a greater accuracy. In lower dimensions, you'll easily overshoot the marked state and so can't get much closer to it
    private const int MinNQubits = 1;
    private const int MaxNQubits = 16;

    // How many times to run for each number of qubits
    private const int NAttempts = 1000;

    private static readonly Random Rnd = new();

    public static void Main()
    {
        Console.WriteLine("Grover's Algorithm Simulation\n\n");
        Console.WriteLine("Number of simulated qubits | Percentage accuracy");

        for (var nQubits = MinNQubits; nQubits <= MaxNQubits; nQubits++)
        {
            var maxVal = (int)Math.Pow(2, nQubits) - 1;
            var answer = Rnd.Next(1, maxVal);
            var results = Enumerable.Range(0, NAttempts).Select(_ => DoGroversAlgorithm(nQubits, answer)).ToArray();
            Console.WriteLine($"{nQubits} | {Math.Round(results.Count(r => r) / (double)NAttempts * 100, 2)}%");
        }
    }

    private static bool DoGroversAlgorithm(int nQubits, int answer)
    {
        var nStates = Math.Pow(2, nQubits); // Max number of potential outcomes
        var nIterations = (int)(Math.PI / 4.0 * Math.Sqrt(nStates)); // Any more than this and you'll overshoot the marked state
        var balancedStateValue = 1 / Math.Sqrt(nStates); // Start with an equal probability of any outcome

        // Just a unit vector representing the correct answer. Right now, we're statically defining this.
        // A real quantum computer wouldn't actually compute the correct value in advance.
        // The oracle would implement some quantum logic that can flip the sign of the marked state(s) based on the properties of the quantum state.
        var markedStateVector = new double[(int)nStates];
        markedStateVector[answer] = 1;

        // Initialise our state vectors
        var uniformStateVector = new double[(int)nStates];
        var stateVector = new double[(int)nStates];
        for (var i = 0; i < nStates; i++)
        {
            uniformStateVector[i] = balancedStateValue;
            stateVector[i] = balancedStateValue;
        }

        for (var i = 0; i < nIterations; i++)
        {
            // Flip the amplitude (sign) for values matching the predicate
            stateVector = Oracle(stateVector, markedStateVector);

            // Reflect across the uniform state vector
            stateVector = Diffuse(stateVector, uniformStateVector);
        }

        // Squaring each value in the state vector gives our probability distribution
        var probabilityDistribution = stateVector.Select(v => Math.Pow(v, 2)).ToArray();
        var total = probabilityDistribution.Sum();

        // Should only be needed to correct errors from deviation caused by large numbers of iterations in floating point arithmetic
        var normalisedProbabilityDistribution = probabilityDistribution.Select((v, i) => v / total).ToArray();
        var cumulativeDistribution = new double[(int)nStates];
        for (var i = 0; i < normalisedProbabilityDistribution.Length; i++)
        {
            cumulativeDistribution[i] = normalisedProbabilityDistribution[i] + (i > 0 ? cumulativeDistribution[i - 1] : 0);
        }

        var val = Rnd.NextDouble();
        for (var i = 0; i < cumulativeDistribution.Length; i++)
        {
            if (!(cumulativeDistribution[i] >= val)) continue;
            return i == answer;
        }

        return false;
    }

    /*
     * Mathematically: ∣ψ⟩ ↦ D∣ψ⟩=∣ψ⟩+2⟨ψ0∣ψ⟩∣ψ0⟩
     * - D is the Diffusion operator
     * - ∣ψ0⟩ is the initial state vector
     * - ∣ψ⟩ is the state vector
     *
     * Reflects ∣ψ⟩ across the plane orthogonal to ∣ψ0⟩
     *
     * Also represented as D=2∣ψ0⟩⟨ψ0∣−I
     * - I is the corresponding identity matrix
     *
     * E.g For a 2 qubit example (4 basis states)
     * 1. You'd multiply the column state vector the conjugate transpose of the state vector (we're only using real numbers here though)
     *     E.g.
     *     [
     *       1 + i
     *       2 - i        ==>  [ 1 - i, 2 + i ]
     *     ]
     *
     * 2. This would give you a 4 x 4 matrix, which you'd double and subtract the identity from
     * 3. You'd multiply your state vector by this
     *
     * The two representations are mathematically equivalent.
    */
    private static double[] Diffuse(double[] stateVector, double[] uniformStateVector)
    {
        var dotProduct = stateVector.Select((t, i) => uniformStateVector[i] * t).Sum();

        var projectionOntoUniform = uniformStateVector.Select(v => v * dotProduct).ToArray();

        return stateVector.Select((v, i) => (v - 2.0 * projectionOntoUniform[i]) * -1).ToArray();
    }

    /*
     * Mathematically: ∣ψ⟩ ↦ O∣ψ⟩=∣ψ⟩−2⟨w∣ψ⟩∣w⟩
     * - O is the Oracle operator
     * - ∣w⟩ is the marked state vector (the right answer)
     * - ∣ψ⟩ is the state vector
     *
     * Reflects ∣ψ⟩ across the plane orthogonal to ∣w⟩
    */
    private static double[] Oracle(double[] stateVector, double[] markedStateVector)
    {
        var dotProduct = stateVector.Select((t, i) => markedStateVector[i] * t).Sum();

        var projectionOntoMarked = markedStateVector.Select(v => v * dotProduct).ToArray();

        return stateVector.Select((v, i) => v - 2.0 * projectionOntoMarked[i]).ToArray();
    }
}
