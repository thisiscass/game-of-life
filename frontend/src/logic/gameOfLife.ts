/**
 * right   
 * bottom-right
 * bottom  
 * bottom-left 
 * left
 * top-left
 * top
 * top-right
 */
const navigation = [
    [0, 1],
    [1, 1],
    [1, 0],
    [1, -1],
    [0, -1],
    [-1, -1],
    [-1, 0],
    [-1, 1]
];

export type Grid = number[][]

export const buildEmptyGrid = (rows: number, columns: number): Grid => {
    return Array.from({ length: rows }, () => Array(columns).fill(0));
}

export const isGridEmpty = (grid: Grid) => grid.every(row => row.every(square => square === 0));

export function buildNextGeneration(grid: Grid): Grid {
    const rowsLength = grid.length;
    const columnsLength = grid[0].length;

    const newGrid = buildEmptyGrid(rowsLength, columnsLength);

    for (let i = 0; i < rowsLength; i++) {
        for (let j = 0; j < columnsLength; j++) {
            let neighbours = 0;

            for (let [x, y] of navigation) {
                let currentrow = i + x;
                let currentcol = j + y;

                if (currentrow >= 0 &&
                    currentrow < rowsLength &&
                    currentcol >= 0 &&
                    currentcol < columnsLength) {

                    try {
                        if (grid[currentrow][currentcol] == 1)
                            neighbours += 1;
                    } catch (error) {
                        console.log("Out of range")
                    }

                }
            }

            if (grid[i][j] == 0 && neighbours == 3) {
                newGrid[i][j] = 1;
            } else if (grid[i][j] == 1 && (neighbours == 2 || neighbours == 3)) {
                newGrid[i][j] = 1;
            } else {
                newGrid[i][j] = 0;
            }
        }
    }

    return newGrid;
}