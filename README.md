#
# Goal-Oriented Action Planning
The project was developed within the scope of a bachelor thesis as part of the study program Animation&Game at the Hochschule Darmstadt. 
Visual assets such as 3D models, textures etc. were developed by third party.

## Directories/Folder structure
The project contains three GOAP solutions.
- Solution no.1 runs on the main thread - used by the sample agent.
    - .../Assets/Scripts/GOAP_Collections/Native
- Solution no.2 uses the job system contained in the Unity ECS.
    - .../Assets/Scripts/GOAP_Collections/DOTS
- Solution no.3 is still in development. It is an extension to the first solution and is supposed to provide editor tools to author and debug the systems.
    - .../Assets/Scripts/GOAP_Collections/GOAP_Debugging_Support

### Important directories for each solution
- Base classes to run GOAP .../Base
- Utility classes such as a priority queue .../Utility
- Example actions and agents .../Agents

###In order create and extend an agent:
1. Create a class 'Action' : Action<Agent>
    - Provide preconditions and effects - e.g.: Preconditions.Add("Hungry", false)
2. Create a class of type Agent
3. Provide one or more instances of class WorldState through Agent.CreateWorldState(params) or Agent.CreateGoal(params) to represent the current world state and goals
    - e.g.: worldStateInstance.Add("Hungry", true) | eatGoal.Add("Hungry", false)
4. Add actions through Agent.AddAction(action)
5. Within an instance of the class GOAPAgent call Agent.Plan(StringBuilder, cancellable)

A full example is provided in Assets/Scripts/GOAP_Collections/Native/Agents
