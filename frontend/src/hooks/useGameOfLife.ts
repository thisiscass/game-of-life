import { useEffect, useRef, useState } from "react";
import { buildEmptyGrid, buildNextGeneration as buildNextGenerationGrid, isGridEmpty, type Grid } from "../logic/gameOfLife";

export const useGameOfLife = (rows: number, cols: number) => {
    const [grid, setGrid] = useState<Grid>(buildEmptyGrid(rows, cols));
    const [generation, setGeneration] = useState(0);
    const [isRunning, setIsRunning] = useState(false);
    const intervalRef = useRef<number | null>(null);

    const toggleSquare = (r: number, c: number) => {
        setGrid(previousGrid => {
            const newGrid = previousGrid.map(row => [...row]);
            newGrid[r][c] = previousGrid[r][c] ? 0 : 1;

            const empty = isGridEmpty(previousGrid);

            if (empty && newGrid[r][c] === 1) {
                setGeneration(0);
            }

            return newGrid;
        });
    };

    const start = () => setIsRunning(true);
    const stop = () => setIsRunning(false);
    const clear = () => {
        setGrid(buildEmptyGrid(rows, cols));
        setGeneration(0);
    }

    const advanceGenerations = (steps: number) => {
        let tempGrid = grid.map(row => [...row]);
        let tempGeneration = generation;

        start();

        for (let i = 0; i < steps; i++) {
            tempGrid = buildNextGenerationGrid(tempGrid);
            tempGeneration += 1;
        }
        stop();

        setGrid(tempGrid);
        setGeneration(tempGeneration);
    };

    useEffect(() => {
        if (isRunning) {
            intervalRef.current = window.setInterval(() => {
                setGrid(currentGrid => {
                    const nextGenGrid = buildNextGenerationGrid(currentGrid);

                    setGeneration(gen => gen + 1);
                    return nextGenGrid;
                });
            }, 500)
        } else if (intervalRef.current) {
            clearInterval(intervalRef.current);
            intervalRef.current = null;
        }

        return () => {
            if (intervalRef.current) clearInterval(intervalRef.current);
        };
    }, [isRunning]);

    return {
        grid,
        toggleSquare,
        start,
        stop,
        clear,
        isRunning,
        generation,
        advanceGenerations
    };
};
