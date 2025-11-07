## NavMesh Plus Setup Instructions

### 1. Scene Setup
1. Create an object with Navmesh Surface component
2. Add a Navigation CollectSources2d component to that object and click 'Rotate to XY'
3. Create tilemaps with appropriate navigation modifiers
   - Check the 'Override area' field
   - Select the appropriate option from the Area dropdown ('Walkable', 'Not Walkable')
4. Bake the NavMesh from the surface component

### 2. Making Changes
 - You will need to rebake the Navmesh everytime changes to the tilemaps are made.
 - To change NavMesh padding: reduce or increase the agent's radius (Window -> AI -> Navigation -> Agent Type -> Radius).
 - In most cases, try to design hallways with at least two tiles in mind to leave enough room for navigation.