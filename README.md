# ⎨💠 Welcome to QuBox™! 💠⎬ <img src=QuLangProcessor/qubox-logo.png align=left width=200px />

Quantum computing is a rapidly-emerging technological field at the junction of computer science and quantum mechanics. The topic aims to revolutionize research relying on heavy computation and leverage some of the physical limitations of classical supercomputers. The revised computation model stores and manipulates information on `qubits`, as  a replacement of classical binary bits that are intrinsically part of the devices we use today.

The study of quantum computing attempts to out-perform the capabilities of classical supercomputers in solving specific classes of problems that can encode useful information in the underlying computation model. Conceptual examples extend beyond universal Turing computation to Quantum Key Distribution (QKD) and Quantum Machine Learning (QML), with an exciting application of Shor’s algorithm for efficient factorization. Therefore, it should be noted that quantum computers are not universally faster than their classical counterparts. The performance advantage would then come from an exponentially smaller number of operations needed to achieve the result, rather than the execution time of each operation.

# Quick Links:
| [Quantum Information](#quantum-information) | 
[Objectives](#objectives) | 
[Design]() |

# Quantum Information:

The possibility of performing a large number of operations in parallel (quantum parallelism), is established on 2 fundamental properties of quantum states: superposition and entanglement. The basic unit of quantum information is the `qubit` that can exist in multiple intermediate states as linear superpositions of basis states — 0 and 1.  Entanglement means in essence that under some circumstances 2 units can form a single entity and any attempt to view or separate the entity into a combination of the 2 units fails by breaking that internal correlation. Thus, the state of one entity affects the state of the other.

Quantum computing relies on the computation model of uniform reversible circuits, which is equivalent to the universal Turing machine and is expressed in terms of a series of matrix-vector multiplications. Any irreversible computation can be made reversible by adding additional qubit channels if needed. Reversible operations are implied by a quantum computing principle that assures the evolution of a quatum system is unitary. Most quantum operators are considered their own inverses and referred to as involutory matrices. 

The vector representation of a qubit is given below:
```math
  \ket{\theta} = \cos \theta \ket{0} + \sin \theta \ket{1}, \text{~where~} \ket{0}=\begin{pmatrix} 1 \\ 0 \end{pmatrix}, \ket{1}=\begin{pmatrix} 0 \\ 1 \end{pmatrix}
```
Multiple qubits can be combined into a single entity by use of the `tensor product` - similar to the `Kronecker product`:
```math
  \ket{\alpha} \otimes \ket{\beta} = \ket{\alpha \beta}
```
The state-vector size of such systems grows exponentially with the number of qubits included $dim(\mathcal{H})=2^{n}$.
Quantum operators (gates) are physical devices that perform gentle touches to the state of the qubit, without collapsing it. The `Hadamard` gate allows the transition from a classical bit (base state) to one in perfectly equal superposition and the other way around without the need for a measurement. The `X` gate is similar to the negation (bit-flip) operation performed on classical bits and belongs to the family of `Pauli` gates. The analogous operation to the `NAND` used in modern computers is the `CNOT` (control-not). Whenever the control bit is `1` a bit-flip operation is triggered on the target bit, otherwise remaining unchanged. Although, when the control bit is in perfect superposition we obtain the special effect of `quantum entanglement` between 2 qubits. It is a form of intrinsic correlation that has no limit in space and time. It does not allow us to factorize the state of a quantum system into separate qubit states and any measurement collapse on one of the involved qubits inflicts a change in the state of the other qubit. Einstein referred to this phenomenon as a `Spooky action at a distance`.

# Objectives:

The development of quantum computing is for now stuck in the era of the noisy intermediate-scale quantum (NISQ) paradigm. The primary ambition for the future post-NISQ era is to develop a fault-tolerant scalable quantum device, able to resemble the universal quantum machine - run any and all quantum algorithms and correct errors faster than the rate at which they occur. This expectation is based on the principle of quantum error correction, that essentially implies that in order to protect the quantum system from environmental damage, encoding it in a very highly entangled state is necessary. 

Real quantum machines are not readily available at all times due to high operational costs and limited resources, hence hardware providers impose various cloud runtimes to maintain the efficient scheduling of quantum jobs. Quantum composers however allow prompt executions of quantum algorithms on classical computers, with an exponentially increasing complexity - reducing the number of qubits modelled in the system. Despite the limitations, such simulators prove beneficial in alleviating cost and complexity for the development and testing of low-scale circuits.

In the modern context of the field, there is a considerable need for web platforms where users can design and test out quantum circuits. With this kind of tools, researchers can quickly iterate and improve designs of developing algorithms, protocols and circuits, ultimately leading to faster progress in the field. With supportive interfaces, the accesibility of the research topic is broadened to a wider audience, abstracting away some of the underlying sophistication.

The primary objective of the platform we are providing is to remove the complexities associated with working in a high-level quantum framework and provide a static client-side solution that can run in the web-browser at any given time - online or locally. The open-source projects will ensure a straightforward extension of the platform with new functionality using a modular and patterned structure.

# Platform Architecture:
```  
  ... to be continued ...
```
