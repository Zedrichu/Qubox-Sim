# ⎨💠 Welcome to QuBox™! 💠⎬

Quantum computing is a rapidly-emerging technological field at the junction of computer science and quantum mechanics. The topic aims to revolutionize research relying on heavy computation and leverage some of the physical limitations of classical supercomputers. The revised computation model stores and manipulates information on `qubits`, as  a replacement of classical binary bits that are intrinsically part of the devices we use today.

The study of quantum computing attempts to out-perform the capabilities of classical supercomputers in solving specific classes of problems that can encode useful information in the underlying computation model. Conceptual examples extend beyond universal Turing computation to Quantum Key Distribution (QKD) and Quantum Machine Learning (QML), with an exciting application of Shor’s algorithm for efficient factorization. Therefore, it should be noted that quantum computers are not universally faster than their classical counterparts. The performance advantage would then come from an exponentially smaller number of operations needed to achieve the result, rather than the execution time of each operation.

# Quick Links:
| [Quantum Information](#quantum-information) | 
[Objectives](#objectives) | 
[Design]() |

# Quantum Information:

The possibility of performing a large number of operations in parallel (quantum parallelism), is established on 2 fundamental properties of quantum states: superposition and entanglement. The basic unit of quantum information is the `qubit` that can exist in multiple intermediate states as linear superpositions of basis states — 0 and 1.  Entanglement means in essence that under some circumstances 2 units can form a single entity and any attempt to view or separate the entity into a combination of the 2 units fails by breaking that internal correlation. Thus, the state of one entity affects the state of the other.

# Objectives:

The development of quantum computing is for now stuck in the era of the noisy intermediate-scale quantum (NISQ) paradigm. The primary ambition for the future post-NISQ era is to develop a fault-tolerant scalable quantum device, able to resemble the universal quantum machine - run any and all quantum algorithms and correct errors faster than the rate at which they occur. This expectation is based on the principle of quantum error correction, that essentially implies that in order to protect the quantum system from environmental damage, encoding it in a very highly entangled state is necessary. 

Real quantum machines are not readily available at all times due to high operational costs and limited resources, hence hardware providers impose various cloud runtimes to maintain the efficient scheduling of quantum jobs. Quantum composers however allow prompt executions of quantum algorithms on classical computers, with an exponentially increasing complexity - reducing the number of qubits modelled in the system. Despite the limitations, such simulators prove beneficial in alleviating cost and complexity for the development and testing of low-scale circuits.

In the modern context of the field, there is a considerable need for web platforms where users can design and test out quantum circuits. With this kind of tools, researchers can quickly iterate and improve designs of developing algorithms, protocols and circuits, ultimately leading to faster progress in the field. With supportive interfaces, the accesibility of the research topic is broadened to a wider audience, abstracting away some of the underlying sophistication.

The primary objective of the platform we are providing is to remove the complexities associated with working in a high-level quantum framework and provide a static client-side solution that can run in the web-browser at any given time - online or locally. The open-source projects will ensure a straightforward extension of the platform with new functionality using a modular and patterned structure.
