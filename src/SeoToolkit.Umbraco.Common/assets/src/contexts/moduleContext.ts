import { createContext } from '@lit/context';
import { ModuleRepository } from '../repositories/moduleRepository';
export const moduleContext = createContext<ModuleRepository>("moduleRepository");