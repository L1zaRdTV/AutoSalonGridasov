import { cars } from '../data/cars.js';

const byKey = new Map();

function createKey(brand, model) {
    return `${brand}`.trim().toLowerCase() + '::' + `${model}`.trim().toLowerCase();
}

export function initCarsRegistry() {
    byKey.clear();

    cars.forEach((car) => {
        byKey.set(createKey(car.brand, car.model), car);
    });
}

export function getCarData(brand, model) {
    return byKey.get(createKey(brand, model)) ?? null;
}
