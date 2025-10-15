import { useState } from "react";
import {
    getNextBoard,
    createBoard as createNewBoard,
    advanceNSteps,
    start as startGame,
    stop as stopGame
} from "../api/gameOfLifeApi";

export function useGameOfLife(rows: number, cols: number) {
    const [grid, setGrid] = useState<number[][]>(
        Array.from({ length: rows }, () => Array(cols).fill(0))
    );
    const [isRunning, setIsRunning] = useState(false);
    const [generation, setGeneration] = useState(0);
    const [boardId, setBoardId] = useState<string>("");

    async function advanceSteps(steps: number) {
        if (steps <= 0) return;
        try {
            let id = boardId;
            if (!id) {
                id = await createBoard() ?? "";
            }

            if (!id) throw new Error("Board was not created properly.");
            await advanceNSteps(id, steps);

        } catch (error) {
            console.log("Failed to advance N steps.", error);
        }
    }


    async function createBoard() {
        try {
            const response = await createNewBoard({ grid: grid });

            setBoardId(response);

            return response;
        } catch (err) {
            console.error("Failed to create board:", err);
        }
    }

    async function nextGeneration() {
        try {
            let id = boardId;
            if (!id) {
                id = await createBoard() ?? "";
            }

            if (!id) throw new Error("Board was not created properly.");

            const result = await getNextBoard(id);
            setGrid(result.grid);
            setGeneration(result.generation);
        } catch (err) {
            console.log("Error fetching next generation", err);
        }
    }

    function toggleSquare(r: number, c: number) {
        const newGrid = grid.map((row, i) =>
            row.map((cell, j) => (i === r && j === c ? (cell ? 0 : 1) : cell))
        );

        if (generation > 0) {
            setGeneration(0)
            setBoardId("");
        }

        setGrid(newGrid);
    }

    function clear() {
        setGrid(Array.from({ length: rows }, () => Array(cols).fill(0)));
        setGeneration(0);
        setBoardId("");
    }

    async function start() {
        let id = boardId;
        if (!id) {
            id = await createBoard() ?? "";
        }

        if (!id) throw new Error("Board was not created properly.");

        setIsRunning(true);

        startGame(id);
    }

    function stop() {
        setIsRunning(false);

        stopGame(boardId);
    }

    return {
        grid,
        toggleSquare,
        clear,
        isRunning,
        start,
        stop,
        generation,
        createBoard,
        advanceSteps,
        nextGeneration,
    };
}


// export const useGameOfLife = (rows: number, cols: number) => {
//     const [grid, setGrid] = useState<Grid>(buildEmptyGrid(rows, cols));
//     const [generation, setGeneration] = useState(0);
//     const [isRunning, setIsRunning] = useState(false);
//     const intervalRef = useRef<number | null>(null);

//     const toggleSquare = (r: number, c: number) => {
//         setGrid(previousGrid => {
//             const newGrid = previousGrid.map(row => [...row]);
//             newGrid[r][c] = previousGrid[r][c] ? 0 : 1;

//             const empty = isGridEmpty(previousGrid);

//             if (empty && newGrid[r][c] === 1) {
//                 setGeneration(0);
//             }

//             return newGrid;
//         });
//     };

//     const start = () => setIsRunning(true);
//     const stop = () => setIsRunning(false);
//     const clear = () => {
//         setGrid(buildEmptyGrid(rows, cols));
//         setGeneration(0);
//     }

//     const advanceGenerations = (steps: number) => {
//         let tempGrid = grid.map(row => [...row]);
//         let tempGeneration = generation;

//         start();

//         for (let i = 0; i < steps; i++) {
//             tempGrid = buildNextGenerationGrid(tempGrid);
//             tempGeneration += 1;
//         }
//         stop();

//         setGrid(tempGrid);
//         setGeneration(tempGeneration);
//     };

//     useEffect(() => {
//         if (isRunning) {
//             intervalRef.current = window.setInterval(() => {
//                 setGrid(currentGrid => {
//                     const nextGenGrid = buildNextGenerationGrid(currentGrid);

//                     setGeneration(gen => gen + 1);
//                     return nextGenGrid;
//                 });
//             }, 500)
//         } else if (intervalRef.current) {
//             clearInterval(intervalRef.current);
//             intervalRef.current = null;
//         }

//         return () => {
//             if (intervalRef.current) clearInterval(intervalRef.current);
//         };
//     }, [isRunning]);

//     return {
//         grid,
//         toggleSquare,
//         start,
//         stop,
//         clear,
//         isRunning,
//         generation,
//         advanceGenerations
//     };
// };
